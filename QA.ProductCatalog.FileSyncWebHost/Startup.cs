using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QA.Core.Logger;
using QA.DPC.Core.Helpers;
using Swashbuckle.AspNetCore.Swagger;
using ILogger = QA.Core.Logger.ILogger;

namespace QA.ProductCatalog.FileSyncWebHost
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
            
            services.Configure<DataOptions>(Configuration.GetSection("Data"));
            services.AddScoped<ILogger>(logger => new NLogLogger("NLogClient.config"));
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "DPC FileSyncWebHost API", 
                    Version = "v1",
                    Description = "This API gives access to reference fronts"
                });
            });            
            
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DPC FileSyncWebHost API");
            });
        }
    }
}