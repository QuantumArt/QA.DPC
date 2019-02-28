using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using QA.Core.DPC.QP.Models;
using QA.Core.ProductCatalog.Actions;
using QA.DPC.Core.Helpers;
using QA.ProductCatalog.Integration;
using QA.ProductCatalog.WebApi.App_Start;
using QA.Core.DPC.Formatters.Formatting;
using QA.Core.DPC.Formatters.Services;
using QA.Core.DPC.Loader;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using Swashbuckle.AspNetCore.Swagger;
using Unity;

namespace QA.ProductCatalog.WebApi
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
            
            services.Configure<ConnectionProperties>(Configuration.GetSection("Connection"));
            services.Configure<LoaderProperties>(Configuration.GetSection("Loader"));
            services.Configure<IntegrationProperties>(Configuration.GetSection("Integration"));
            
            services.Configure<RouteOptions>(options =>
            {
                options.ConstraintMap.Add("media_type_mapping", typeof(MediaTypeMappingConstraint));
            });

            var sp = services.BuildServiceProvider();
            services
                .AddMvc(options => { SetupMvcOptions(options, sp); } )
                .AddXmlSerializerFormatters()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
            });
           
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
                app.UseExceptionHandler(new GlobalExceptionHandler(loggerFactory).Action);
            }
            
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseMvcWithDefaultRoute();
        }

        private static void SetupMvcOptions(MvcOptions options, ServiceProvider sp)
        {
            options.FormatterMappings.SetMediaTypeMappingForFormat(WebApiConfig.XmlMappingValue,
                MediaTypeHeaderValue.Parse(WebApiConfig.XmlMediaType));
            options.FormatterMappings.SetMediaTypeMappingForFormat(WebApiConfig.JsonMappingValue,
                MediaTypeHeaderValue.Parse(WebApiConfig.JsonMediaType));
            options.FormatterMappings.SetMediaTypeMappingForFormat(WebApiConfig.PdfMappingValue,
                MediaTypeHeaderValue.Parse(WebApiConfig.PdfMediaType));
            options.FormatterMappings.SetMediaTypeMappingForFormat(WebApiConfig.XamlMappingValue,
                MediaTypeHeaderValue.Parse(WebApiConfig.XamlMediaType));
            options.FormatterMappings.SetMediaTypeMappingForFormat(WebApiConfig.JsonDefinitionMappingValue,
                MediaTypeHeaderValue.Parse(WebApiConfig.JsonDefinitionMediaType));
            options.FormatterMappings.SetMediaTypeMappingForFormat(WebApiConfig.JsonDefinition2MappingValue,
                MediaTypeHeaderValue.Parse(WebApiConfig.JsonDefinition2MediaType));
            options.FormatterMappings.SetMediaTypeMappingForFormat(WebApiConfig.BinaryMappingValue,
                MediaTypeHeaderValue.Parse(WebApiConfig.BinaryMediaType));
            
            var jsonOutputFormatter = options.OutputFormatters.OfType<JsonOutputFormatter>().FirstOrDefault();
            if (jsonOutputFormatter != null)
            {
                options.OutputFormatters.Remove(jsonOutputFormatter);
            }
            
            options.OutputFormatters.Add(new ModelMediaTypeOutputFormatter<Article, XmlProductFormatter>(WebApiConfig.XmlMediaType));
            options.OutputFormatters.Add(new ModelMediaTypeOutputFormatter<Article, XamlProductFormatter>(WebApiConfig.XamlMediaType));            
            options.OutputFormatters.Add(new ModelMediaTypeOutputFormatter<Article, JsonProductFormatter>(WebApiConfig.JsonMediaType));
#if !NETSTANDARD
            options.OutputFormatters.Add(new ModelMediaTypeOutputFormatter<Article, PdfProductFormatter>(WebApiConfig.PdfMediaType));
#endif
            options.OutputFormatters.Add(new ModelMediaTypeOutputFormatter<IEnumerable<Article>, JsonProductArrayFormatter>(WebApiConfig.JsonMediaType));
            options.OutputFormatters.Add(new ModelMediaTypeOutputFormatter<Content, XamlSchemaFormatter>(WebApiConfig.XamlMediaType));
            options.OutputFormatters.Add(new ModelMediaTypeOutputFormatter<Content, XmlSchemaFormatter>(WebApiConfig.XmlMediaType));
            options.OutputFormatters.Add(new ModelMediaTypeOutputFormatter<Content, JsonSchemaFormatter>(WebApiConfig.JsonMediaType));
            options.OutputFormatters.Add(
                new ModelMediaTypeOutputFormatter<Content, JsonDefinitionSchemaFormatter>(
                    WebApiConfig.JsonDefinitionMediaType)
                );
            options.OutputFormatters.Add(
                new ModelMediaTypeOutputFormatter<Content, JsonDefinitionSchemaClassifiersAsBackwardsFormatter>(
                    WebApiConfig.JsonDefinition2MediaType)
                );

            if (jsonOutputFormatter != null)
            {
                options.OutputFormatters.Add(jsonOutputFormatter);             
            }
        }
    }
}