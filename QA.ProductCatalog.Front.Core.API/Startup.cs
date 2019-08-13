﻿using System;
 using System.Collections.Generic;
 using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QA.Core.DPC.Front;
using QA.Core.DPC.Front.DAL;
 using QA.Core.DPC.QP.Models;
 using QA.Core.Logger;
using QA.DPC.Core.Helpers;
 using QP.ConfigurationService.Models;
 using ILogger = QA.Core.Logger.ILogger;
using Swashbuckle.AspNetCore.Swagger;

namespace QA.ProductCatalog.Front.Core.API
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
            // Add framework services.
            services.AddMvc(opts =>
            {
                opts.InputFormatters.RemoveType<JsonInputFormatter>();
                opts.InputFormatters.Add(new TextUniversalInputFormatter());
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);;
            
            var dataOptions = new DataOptions();
            Configuration.Bind("Data", dataOptions);
            services.AddSingleton(dataOptions);
            services.AddHttpContextAccessor();
            services.AddScoped<ConnectionService>();

            services.AddDbContext<NpgSqlDpcModelDataContext>(options =>
                options.UseNpgsql(dataOptions.DesignConnectionString));
            
            services.AddDbContext<SqlServerDpcModelDataContext>(options =>
                options.UseSqlServer(dataOptions.DesignConnectionString));

            services.AddScoped<DpcModelDataContext>(sp =>
                {
                    var customer = sp.GetRequiredService<ConnectionService>().GetCustomer().Result;
                    if (customer.DatabaseType == DatabaseType.Postgres)
                    {
                        return GetNpgSqlDpcModelDataContext(customer.ConnectionString);
                    }
                    return GetSqlServerDpcModelDataContext(customer.ConnectionString);
                });

            services.AddScoped<ILogger>(logger => new NLogLogger("NLog.config"));
            services.AddScoped(typeof(IDpcProductService), typeof(DpcProductService));
            services.AddScoped(typeof(IDpcService), typeof(DpcProductService));

            services.Configure<IntegrationProperties>(Configuration.GetSection("Integration"));

            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "DPC Front API", 
                    Version = "v1",
                    Description = "This API gives access to reference fronts"
                });
            });            
            
        }

        private static DpcModelDataContext GetNpgSqlDpcModelDataContext(string connectionString)
        {
            var builder = new DbContextOptionsBuilder<NpgSqlDpcModelDataContext>();
            builder.UseNpgsql(connectionString);
            return new NpgSqlDpcModelDataContext(builder.Options);
        }

        private static DpcModelDataContext GetSqlServerDpcModelDataContext(string connectionString)
        {
            var builder = new DbContextOptionsBuilder<SqlServerDpcModelDataContext>();
            builder.UseSqlServer(connectionString);
            return new SqlServerDpcModelDataContext(builder.Options);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
                
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(new GlobalExceptionHandler(loggerFactory).Action);
            }

            app.UseMvcWithDefaultRoute();
            
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DPC Front API");
            });
        }
    }
}
