using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QA.Core.DPC.Loader;
using QA.Core.DPC.QP.Models;
using QA.DPC.Core.Helpers;
using QA.ProductCatalog.Admin.WebApp.Binders;
using QA.ProductCatalog.Integration;
using Swashbuckle.AspNetCore.Swagger;
using Unity;

namespace QA.ProductCatalog.Admin.WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureContainer(IUnityContainer container)
        {
            var loaderProps = new LoaderProperties();
            Configuration.Bind("Loader", loaderProps);
            
            var props = new Properties();
            Configuration.Bind("Properties", props);            
            
            UnityConfig.Configure(container, loaderProps, props);
        }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddHttpContextAccessor();
            services.AddTransient<IActionContextAccessor, ActionContextAccessor>();
            services.AddTransient<QPHelper>();
            
            services.Configure<ConnectionProperties>(Configuration.GetSection("Connection"));
            services.Configure<LoaderProperties>(Configuration.GetSection("Loader"));
            services.Configure<IntegrationProperties>(Configuration.GetSection("Integration"));
            services.Configure<QPOptions>(Configuration.GetSection("QP"));        
            
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
            });

            
            services
                .AddMvc(options =>
                    {
                        options.ModelBinderProviders.Insert(0, new RemoteValidationContextModelBinderProvider());
                        options.ModelBinderProviders.Insert(0, new ContentBinderProvider());
                    })
                .AddXmlSerializerFormatters()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
           
        }
        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(new GlobalExceptionHandler(loggerFactory).Action);            }

            app.UseStaticFiles();
            app.UseSession();
            app.UseMvc(routes =>
            {
                routes.MapRoute("1", "{controller=Home}/publicate/preaction/{id?}");
                routes.MapRoute("2", "{controller=Home}/clone/preaction/{id?}");
                routes.MapRoute("3", "{controller=Home}/send/preaction/{id?}");
                routes.MapRoute("NonInterfaceAction", "Action/{command}");
                routes.MapRoute("RemoteValidation", "RemoteValidation/{validatorKey}");
                routes.MapRoute("Default", "{controller=Home}/{action=Index}/{id?}");
            });
        }

    }
}