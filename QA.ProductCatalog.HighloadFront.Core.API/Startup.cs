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
using QA.DPC.Core.Helpers;
using QA.ProductCatalog.HighloadFront.Core.API.DI;
using QA.ProductCatalog.HighloadFront.Core.API.Filters;
using QA.ProductCatalog.HighloadFront.Options;

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
            
            var opts = new HarvesterOptions();
            Configuration.Bind("Harvester", opts);
            services.AddSingleton(opts);
            
            var opts2 = new SonicElasticStoreOptions();
            Configuration.Bind("SonicElasticStore", opts2);
            services.AddSingleton(opts2);
            
            var opts3 = new DataOptions();
            Configuration.Bind("Data", opts3);
            services.AddSingleton(opts3);
            
            var opts4 = new TaskRunnerDelays();
            Configuration.Bind("ReindexDelays", opts4);
            services.AddSingleton(opts4);
            
            services.AddSingleton(new PolicyRegistry());
            
            services.Configure<IntegrationProperties>(Configuration.GetSection("Integration"));
            
            // Add framework services.
            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
                options.Filters.Add(typeof(ProcessCustomerCodeAttribute));
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });

            services.AddMemoryCache();
            services.AddHttpClient();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule(new DefaultModule() { Configuration = Configuration});
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

