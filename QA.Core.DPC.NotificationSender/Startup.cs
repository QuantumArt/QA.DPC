using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QA.Core.DPC.DAL;
using QA.Core.DPC.Loader;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.Core.ProductCatalog.Actions;
using Unity;
using QA.Core.ProductCatalog.ActionsRunner;
using QA.DPC.Core.Helpers;
using IWebHostEnvironment = Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.OpenApi.Models;

namespace QA.Core.DPC
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
            var props = new NotificationProperties();
            Configuration.Bind("Properties", props);
            UnityConfig.Configure(container, props);
        }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddHttpContextAccessor();
            services.Configure<ConnectionProperties>(Configuration.GetSection("Connection"));
            services.Configure<NotificationProperties>(Configuration.GetSection("Properties"));
            services.Configure<IntegrationProperties>(Configuration.GetSection("Integration"));
            services.AddSingleton<IHostedService, NotificationSender>();
            
            var props = new ConnectionProperties();
            Configuration.Bind("Connection", props);
            if (!String.IsNullOrEmpty(props.DesignConnectionString))
            {
                services.AddDbContext<NpgSqlNotificationsModelDataContext>(options =>
                    options.UseNpgsql(props.DesignConnectionString));
            
                services.AddDbContext<SqlServerNotificationsModelDataContext>(options =>
                    options.UseSqlServer(props.DesignConnectionString));
            }
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Notification API", 
                    Version = "v1",
                    Description = "This API allows to send notifications"
                });
            });

            services
                .AddMvc(opts =>
                {
                    opts.EnableEndpointRouting = false;
                }).AddControllersAsServices();
            
            
            

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
            
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DPC Notification API");
            });

            app.UseMvcWithDefaultRoute();
        }
    }
}