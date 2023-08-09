using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Polly.Registry;
using QA.Core.DPC.QP.Models;
using QA.Core.ProductCatalog.ActionsRunner;
using QA.DotNetCore.Caching;
using QA.DotNetCore.Caching.Interfaces;
using QA.DPC.Core.Helpers;
using QA.ProductCatalog.HighloadFront.Core.API.DI;
using QA.ProductCatalog.HighloadFront.Core.API.Filters;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Options;
using QA.ProductCatalog.TmForum.Extensions;

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
        public IServiceProvider ConfigureServices(IServiceCollection services)
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

            // Add framework services.
            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
                options.Filters.Add(typeof(ProcessCustomerCodeAttribute));
            })
            .AddCustomModules(Configuration, services)
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });

            services.AddMemoryCache();

            services.AddSingleton<ICacheKeyFactory, CacheKeyFactoryBase>();
            services.AddSingleton<ILockFactory, MemoryLockFactory>();
            services.AddSingleton<ICacheProvider, VersionedCacheCoreProvider>();

            services.AddHttpClient();

            services.ResolveTmForumRegistrationForHighloadApi(Configuration);

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule(new DefaultModule() { Configuration = Configuration });
            containerBuilder.Populate(services);
            var container = containerBuilder.Build();
            return new AutofacServiceProvider(container);
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

