using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using Polly;
using Polly.Registry;
using QA.ProductCatalog.ImpactService.API.Services;
using QA.ProductCatalog.ImpactService.API.Helpers;
using Microsoft.Extensions.Hosting;

namespace QA.ProductCatalog.ImpactService.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            services.Configure<ConfigurationOptions>(Configuration);
            
            var props = new ConfigurationOptions();
            Configuration.Bind(props);
            
            services.AddSingleton(new PolicyRegistry());


            services.AddMvc().ConfigureApplicationPartManager(apm =>
            {
                foreach (var library in props.ExtraLibraries ?? new string[] {})
                {
                    var assembly = Assembly.LoadFile(Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory ?? string.Empty,library + ".dll"
                    ));
                    apm.ApplicationParts.Add(new AssemblyPart(assembly));
                }
            });

            services.AddMemoryCache();

            services.AddHttpClient();

            services.AddScoped(typeof(ISearchRepository), typeof(ElasticSearchRepository));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(new GlobalExceptionHandler(loggerFactory).Action);
            }

            app.UseRouting();
            app.UseEndpoints(routes =>
            {
                routes.MapControllers();
            });
            
            LogStart(app, loggerFactory);
        }
        
        
        private void LogStart(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            var config = app.ApplicationServices.GetRequiredService<IConfiguration>();
            var name = config["Name"];
            var logger = loggerFactory.CreateLogger(GetType());
            logger.LogInformation("{appName} started", name);         
        }
    }
}