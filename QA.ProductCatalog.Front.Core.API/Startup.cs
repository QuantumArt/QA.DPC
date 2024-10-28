using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using QA.ProductCatalog.TmForum.Extensions;

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
                opts.EnableEndpointRouting = false;
                opts.InputFormatters.RemoveType<SystemTextJsonInputFormatter>();
                opts.InputFormatters.Add(new TextUniversalInputFormatter());
            });
            
            var dataOptions = new DataOptions();
            Configuration.Bind("Data", dataOptions);
            services.AddSingleton(dataOptions);
            services.AddHttpContextAccessor();
            services.AddScoped<ConnectionService>();

            services.AddDbContext<NpgSqlDpcModelDataContext>(options =>
                options.UseNpgsql(dataOptions.DesignConnectionString));
            
            services.AddDbContext<SqlServerDpcModelDataContext>(options =>
                options.UseSqlServer(dataOptions.DesignConnectionString));

            services.AddScoped(sp =>
                {
                    var customer = sp.GetRequiredService<ConnectionService>().GetCustomer().Result;
                    if (customer.DatabaseType == DatabaseType.Postgres)
                    {
                        return GetNpgSqlDpcModelDataContext(customer.ConnectionString);
                    }
                    return GetSqlServerDpcModelDataContext(customer.ConnectionString, dataOptions.CommandTimeout);
                });

            services.AddScoped<ILogger>(logger => new NLogLogger("NLog.config"));
            services.AddScoped(typeof(IDpcProductService), typeof(DpcProductService));
            services.AddScoped(typeof(IDpcService), typeof(DpcProductService));
            services.AddSingleton<JsonProductSerializer>();
            services.AddSingleton<XmlProductSerializer>();
            services.AddScoped<IProductSerializerFactory, ProductSerializerFactory>();

            services.Configure<IntegrationProperties>(Configuration.GetSection("Integration"));

            services.ResolveTmForumRegistrationForDpcFront(Configuration);
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
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

        private static DpcModelDataContext GetSqlServerDpcModelDataContext(string connectionString, int commandTimeout)
        {
            var builder = new DbContextOptionsBuilder<SqlServerDpcModelDataContext>();
            builder.UseSqlServer(connectionString, options => options.CommandTimeout(commandTimeout));
            
            return new SqlServerDpcModelDataContext(builder.Options);
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

            app.UseMvcWithDefaultRoute();
            
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DPC Front API");
            });
            
            LogStart(app, factory);
        }
        
        private void LogStart(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            var config = app.ApplicationServices.GetRequiredService<IConfiguration>();
            var name = config["Data:Name"];
            var logger = loggerFactory.CreateLogger(GetType());
            logger.LogInformation("{appName} started", name);         
        }
    }
}
