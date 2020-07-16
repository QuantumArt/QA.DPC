using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
            
            var integrationProps = new IntegrationProperties();
            Configuration.Bind("Integration", integrationProps);
            
            UnityConfig.Configure(container, loaderProps, integrationProps, props);
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
            services.AddHttpClient();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.Secure = CookieSecurePolicy.SameAsRequest;
                options.MinimumSameSitePolicy = SameSiteMode.None; 
            }); 
            
            services.AddSession(options =>
            {
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.None;
            });
            
            services.AddResponseCaching();
            services
                .AddMvc(options =>
                    {
                        options.EnableEndpointRouting = false;
                        options.ModelBinderProviders.Insert(0, new ActionContextModelBinderProvider());
                    })
                .AddRazorOptions(options => options.AllowRecompilingViewsOnFileChange = true)
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
            app.UseCookiePolicy();
            app.UseSession();
            app.UseResponseCaching();
            
            app.UseMvcWithDefaultRoute();
            
            LogStart(app, loggerFactory);

        }

        private void LogStart(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            var config = app.ApplicationServices.GetRequiredService<IConfiguration>();
            var name = config["Properties:Name"];
            var logger = loggerFactory.CreateLogger(GetType());
            logger.LogInformation("{appName} started", name);         
        }

    }

}