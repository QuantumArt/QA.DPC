using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly.Registry;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.Core.ProductCatalog.ActionsRunner;
using QA.DotNetCore.Caching;
using QA.DotNetCore.Caching.Interfaces;
using QA.DotNetCore.Engine.Abstractions;
using QA.DotNetCore.Engine.CacheTags;
using QA.DotNetCore.Engine.CacheTags.Configuration;
using QA.DotNetCore.Engine.Persistent.Interfaces;
using QA.DotNetCore.Engine.QpData.Persistent.Dapper;
using QA.DotNetCore.Engine.QpData.Settings;
using QA.DPC.Core.Helpers;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.ContentProviders.Deprecated;
using QA.ProductCatalog.HighloadFront.Core.API.DI;
using QA.ProductCatalog.HighloadFront.Core.API.Filters;
using QA.ProductCatalog.HighloadFront.Core.API.Helpers;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Options;
using QA.ProductCatalog.HighloadFront.PostProcessing;
using QA.ProductCatalog.HighloadFront.Validation;
using QA.ProductCatalog.TmForum.Extensions;
using DatabaseType = QP.ConfigurationService.Models.DatabaseType;

namespace QA.ProductCatalog.HighloadFront.Core.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            var harvesterOptions = new HarvesterOptions();
            Configuration.Bind("Harvester", harvesterOptions);
            services.AddSingleton(harvesterOptions);

            var elasticStoreOptions = new SonicElasticStoreOptions();
            Configuration.Bind("SonicElasticStore", elasticStoreOptions);
            services.AddSingleton(elasticStoreOptions);

            var apiRestrictions = new ApiRestrictionOptions();
            Configuration.Bind("ApiRestrictions", apiRestrictions);
            services.AddSingleton(apiRestrictions);

            var dataOptions = new DataOptions();
            Configuration.Bind("Data", dataOptions);
            services.AddSingleton(dataOptions);

            var taskRunnerDelayOptions = new TaskRunnerDelays();
            Configuration.Bind("ReindexDelays", taskRunnerDelayOptions);
            services.AddSingleton(taskRunnerDelayOptions);

            services.AddSingleton(new PolicyRegistry());
            services.AddSingleton<HashProcessor>();
            services.AddScoped<IProductInfoProvider, ProductInfoProvider>();

            services.Configure<IntegrationProperties>(Configuration.GetSection("Integration"));
            services.Configure<ConnectionProperties>(properties =>
            {
                properties.DpcConnectionString = dataOptions.FixedConnectionString;
                properties.UsePostgres = dataOptions.UsePostgres;
            });

            // Add framework services.
            services.AddMvc(options =>
                {
                    options.EnableEndpointRouting = false;
                    options.Filters.Add(typeof(ProcessCustomerCodeAttribute));
                })
                .AddCustomModules(Configuration, services);

            services.AddMemoryCache();
            services.AddHttpContextAccessor();
            
            services.AddScoped<ProductManager>();
            services.AddScoped<SonicErrorDescriber>();
            services.AddScoped<Func<string, IProductStore>>(c =>
            {
                return version => c.GetRequiredService<Dictionary<string, IProductStore>>()[version];
            });
            services.AddScoped<IProductStoreFactory, ProductStoreFactory>();
            services.AddScoped<ProductImporter>();
            
            services.AddScoped<ElasticProductStore>();
            services.AddScoped<ElasticProductStore_6>();
            services.AddScoped<ElasticProductStore_8>();
            services.AddScoped<OpenSearchProductStore_2>();

            services.AddScoped(c => new Dictionary<string, IProductStore>()
            {
                {"5.*", c.GetRequiredService<ElasticProductStore>()},
                {"6.*", c.GetRequiredService<ElasticProductStore_6>()},
                {"8.*", c.GetRequiredService<ElasticProductStore_8>()},
                {"os2.*", c.GetRequiredService<OpenSearchProductStore_2>()}
            });

            if (!String.IsNullOrEmpty(dataOptions.FixedConnectionString))
            {
                services.AddScoped<ICustomerProvider, SingleCustomerCoreProvider>();
            }
            else
            {
                services.AddScoped<ICustomerProvider, CustomerProvider>();
            }
            
            services.AddScoped<IIdentityProvider>(c => new CoreIdentityProvider(
                c.GetRequiredService<IHttpContextAccessor>(),
                dataOptions.FixedCustomerCode
            ));
            
            services.AddScoped<IConnectionProvider>(c =>
            {
                if (!string.IsNullOrEmpty(dataOptions.FixedConnectionString) || !dataOptions.QpMode)
                    return new ExplicitConnectionProvider(
                        new Customer()
                        {
                            ConnectionString = dataOptions.FixedConnectionString,
                            DatabaseType = dataOptions.GetDatabaseType(),
                            CustomerCode = dataOptions.FixedCustomerCode
                        }
                    );

                return new ConnectionProvider(
                    c.GetRequiredService<ICustomerProvider>(),
                    c.GetRequiredService<IIdentityProvider>(),
                    Service.HighloadAPI
                );
            });

            services.AddSingleton<ICustomerCodeTaskInstanceCollection>(c => new CustomerCodeTaskInstanceCollection(
                c.GetRequiredService<TaskRunnerDelays>(),
                null
            ));
            services.AddScoped(c =>
                {
                    var customerCode = c.GetRequiredService<IConnectionProvider>().GetCustomer().CustomerCode;
                    var configuration = c.GetRequiredService<ElasticConfiguration>();
                    configuration.SetCachePrefix(customerCode);
                    var manager = c.GetRequiredService<ProductManager>();
                    manager.SetCustomerCode(customerCode);
                        
                    return c.GetRequiredService<ICustomerCodeTaskInstanceCollection>().Get(
                        c.GetRequiredService<IIdentityProvider>(),
                        new ReindexAllTask(
                            c.GetRequiredService<ProductImporter>(),
                            manager,
                            configuration,
                            c.GetRequiredService<Dictionary<string, IProductStore>>())).TaskService;
                }
            );

            if (dataOptions.QpMode)
            {
                if (dataOptions.SettingsContentId != 0)
                {
                    services.AddScoped<ISettingsService>(c => new SettingsFromContentCoreService(
                        c.GetRequiredService<IConnectionProvider>(),
                        c.GetRequiredService<ICacheProvider>(),
                        dataOptions.SettingsContentId
                        ));
                }
                else
                {
                    services.AddScoped<ISettingsService, SettingsFromQpCoreService>();
                }
                
                services.AddScoped<IContentProvider<ElasticIndex>, ElasticIndexProvider>();
                services.AddScoped<IContentProvider<HighloadApiLimit>, HighloadApiLimitProvider>();
                services.AddScoped<IContentProvider<HighloadApiUser>, HighloadApiUserProvider>();
                services.AddScoped<IContentProvider<HighloadApiMethod>, HighloadApiMethodProvider>();
                services.AddScoped<ElasticConfiguration, QpElasticConfiguration>();
            }
            else
            {
                services.AddScoped<ElasticConfiguration, JsonElasticConfiguration>();
            }

            services.AddScoped<IProductReadPostProcessor, ContentProcessor>();
            services.AddScoped<IProductReadExpandPostProcessor, ExpandReadProcessor>();
            services.AddScoped<IProductWriteExpandPostProcessor, ExpandWriteProcessor>();

            services.AddScoped<ArrayIndexer>();
            services.AddScoped<DateIndexer>();
            services.AddScoped<IProductWritePostProcessor>(c => new IndexerDecorator(new IProductWritePostProcessor[]
            {
                c.GetRequiredService<ArrayIndexer>(),
                c.GetRequiredService<DateIndexer>()
            }));

            services.AddScoped<ProductsOptionsCommonValidationHelper>();
            
            services.AddSingleton(_ => new CleanCacheLockerServiceSettings()
            {
                RunInterval = dataOptions.CleanKeysOptions.RunInterval,
                CleanInterval = dataOptions.CleanKeysOptions.CleanInterval
            });

            services.AddSingleton(c =>
                {
                    var set = new ServiceSetConfigurator<ICacheTagTracker>();
                    var trackers = c.GetRequiredService<ICustomerProvider>().GetCustomers()
                        .Select(n => new QpContentCacheTracker(
                            c.GetRequiredService<IContentModificationRepository>(),
                            c.GetRequiredService<IQpContentCacheTagNamingProvider>(),
                            () => new UnitOfWork(n.ConnectionString, n.DatabaseType.ToString(), n.CustomerCode)));

                    foreach (var tracker in trackers)
                    {
                        set.RegisterInstance(tracker);
                    }

                    return set;
                }
            );
            
            services.AddCacheTagServices().WithInvalidationByTimer();

            services.AddScoped(QPHelper.CreateUnitOfWork);

            services.TryAddScoped<IMetaInfoRepository, MetaInfoRepository>();
            services.TryAddScoped(c => new QpSiteStructureCacheSettings()
            {
                QpSchemeCachePeriod = TimeSpan.FromMinutes(10)
            });

            services.AddHttpClient();

            services.ResolveTmForumRegistrationForHighloadApi(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory factory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(new GlobalExceptionHandler().Action);
            }

            app.UseMvc();
            
            LogStart(app, factory);
        }
        
        private void LogStart(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            var config = app.ApplicationServices.GetRequiredService<IConfiguration>();
            var syncName = config["Data:SyncName"];
            var searchName = config["Data:SearchName"];
            var canUpdate = bool.TryParse(config["Data:CanUpdate"], out var parsed) && parsed;
            var logger = loggerFactory.CreateLogger(GetType());
            logger.LogInformation("{appName} started", canUpdate ? syncName : searchName);         
        }
    }
}

