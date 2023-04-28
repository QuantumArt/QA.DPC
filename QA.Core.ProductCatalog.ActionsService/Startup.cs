using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QA.Core.DPC.Loader;
using QA.Core.DPC.QP.Models;
using QA.Core.ProductCatalog.Actions;
using Unity;
using QA.Core.ProductCatalog.ActionsRunner;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.DPC.Core.Helpers;
using QA.ProductCatalog.Integration;
using QA.ProductCatalog.TmForum.Extensions;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;


namespace QA.Core.ProductCatalog.ActionsService
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

            UnityConfig.Configure(container, loaderProps);
        }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddHttpContextAccessor();
            services.AddHttpClient();
            services.AddTransient<IActionContextAccessor, ActionContextAccessor>();

            services.Configure<ActionsServiceProperties>(Configuration.GetSection("Properties"));
            services.Configure<ConnectionProperties>(Configuration.GetSection("Connection"));
            services.Configure<LoaderProperties>(Configuration.GetSection("Loader"));
            services.Configure<IntegrationProperties>(Configuration.GetSection("Integration"));

            services.FillQpConfiguration(Configuration);

            services.AddSingleton<ActionsService>();
            services.AddSingleton<IHostedService>(x => x.GetRequiredService<ActionsService>());

            var props = new ConnectionProperties();
            Configuration.Bind("Connection", props);
            if (!String.IsNullOrEmpty(props.DesignConnectionString))
            {
                services.AddDbContext<NpgSqlTaskRunnerEntities>(options =>
                    options.UseNpgsql(props.DesignConnectionString));

                services.AddDbContext<SqlServerTaskRunnerEntities>(options =>
                    options.UseSqlServer(props.DesignConnectionString));
            }

            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
            });;

            services.ResolveTmForumRegistration(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
        }
    }
}
