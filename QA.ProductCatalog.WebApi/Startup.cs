using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
                c.SwaggerDoc("v1", new Info
                {
                    Title = "DPC Web API", 
                    Version = "v1",
                    Description = "This API allows to manipulate products: get schemas, perform CRUD operations"
                });
                
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.XML";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DPC Web API");
            });

            app.UseMvcWithDefaultRoute();
        }

        private static void SetupMvcOptions(MvcOptions options, ServiceProvider sp)
        {
            RegisterMediaTypes(options.FormatterMappings);
            RegisterOutputFormatters(options.OutputFormatters);
            RegisterInputFormatters(options.InputFormatters);
        }
        
        private static void RegisterInputFormatters(FormatterCollection<IInputFormatter> formatters)
        {
            formatters.Insert(0,
                new ModelMediaTypeInputFormatter<Article, JsonProductFormatter>(WebApiConfig.JsonMediaType));
            formatters.Insert(0,
                new ModelMediaTypeInputFormatter<Article, XmlProductFormatter>(WebApiConfig.XmlMediaType));
            formatters.Insert(0,
                new ModelMediaTypeInputFormatter<Article, XamlProductFormatter>(WebApiConfig.XamlMediaType));

                            
            formatters.Add(new BinaryInputFormatter(WebApiConfig.BinaryMediaType));

        }

        private static void RegisterOutputFormatters(FormatterCollection<IOutputFormatter> formatters)
        {
            var jsonOutputFormatter = RemoveDefaultJsonOutputFormatter(formatters);

            RegisterArticleOutputFormatters(formatters);

            RegisterContentOutputFormatters(formatters);

            RegisterAuxOutputFormatters(formatters);

            RestoreDefaultJsonOutputFormatter(formatters, jsonOutputFormatter);
        }

        private static void RestoreDefaultJsonOutputFormatter(FormatterCollection<IOutputFormatter> formatters, JsonOutputFormatter jsonOutputFormatter)
        {
            if (jsonOutputFormatter != null)
            {
                formatters.Add(jsonOutputFormatter);
            }
        }

        private static JsonOutputFormatter RemoveDefaultJsonOutputFormatter(FormatterCollection<IOutputFormatter> formatters)
        {
            var jsonOutputFormatter = formatters.OfType<JsonOutputFormatter>().FirstOrDefault();
            if (jsonOutputFormatter != null)
            {
                formatters.Remove(jsonOutputFormatter);
            }

            return jsonOutputFormatter;
        }

        private static void RegisterAuxOutputFormatters(FormatterCollection<IOutputFormatter> formatters)
        {
            formatters.Add(
                new ModelMediaTypeOutputFormatter<
                    Dictionary<string, object>[], XmlDataContractFormatter<Dictionary<string, object>[]>
                >(WebApiConfig.XmlMediaType)
            );

            formatters.Add(
                new ModelMediaTypeOutputFormatter<
                    IEnumerable<Article>, JsonProductArrayFormatter
                >(WebApiConfig.JsonMediaType)
            );
            
            formatters.Add(
                new ModelMediaTypeOutputFormatter<
                    IEnumerable<Article>, XmlDataContractFormatter<IEnumerable<Article>>
                >(WebApiConfig.XmlMediaType)
            );
            
            formatters.Add(new BinaryOutputFormatter(WebApiConfig.BinaryMediaType));
        }

        private static void RegisterContentOutputFormatters(FormatterCollection<IOutputFormatter> formatters)
        {
            formatters.Add(
                new ModelMediaTypeOutputFormatter<Content, XamlSchemaFormatter>(WebApiConfig.XamlMediaType));
            formatters.Add(
                new ModelMediaTypeOutputFormatter<Content, XmlSchemaFormatter>(WebApiConfig.XmlMediaType));
            formatters.Add(
                new ModelMediaTypeOutputFormatter<Content, JsonSchemaFormatter>(WebApiConfig.JsonMediaType));
            formatters.Add(
                new ModelMediaTypeOutputFormatter<Content, JsonDefinitionSchemaFormatter>(
                    WebApiConfig.JsonDefinitionMediaType)
            );
            formatters.Add(
                new ModelMediaTypeOutputFormatter<Content, JsonDefinitionSchemaClassifiersAsBackwardsFormatter>(
                    WebApiConfig.JsonDefinition2MediaType)
            );
        }

        private static void RegisterArticleOutputFormatters(FormatterCollection<IOutputFormatter> formatters)
        {
            formatters.Add(
                new ModelMediaTypeOutputFormatter<Article, XmlProductFormatter>(WebApiConfig.XmlMediaType));
            formatters.Add(
                new ModelMediaTypeOutputFormatter<Article, XamlProductFormatter>(WebApiConfig.XamlMediaType));
            formatters.Add(
                new ModelMediaTypeOutputFormatter<Article, JsonProductFormatter>(WebApiConfig.JsonMediaType));
#if !NETSTANDARD
            formatters.Add(
                new ModelMediaTypeOutputFormatter<Article, PdfProductFormatter>(WebApiConfig.PdfMediaType));
#endif
        }

        private static void RegisterMediaTypes(FormatterMappings formatterMappings)
        {
            formatterMappings.SetMediaTypeMappingForFormat(WebApiConfig.XmlMappingValue,
                MediaTypeHeaderValue.Parse(WebApiConfig.XmlMediaType));
            formatterMappings.SetMediaTypeMappingForFormat(WebApiConfig.JsonMappingValue,
                MediaTypeHeaderValue.Parse(WebApiConfig.JsonMediaType));
            formatterMappings.SetMediaTypeMappingForFormat(WebApiConfig.PdfMappingValue,
                MediaTypeHeaderValue.Parse(WebApiConfig.PdfMediaType));
            formatterMappings.SetMediaTypeMappingForFormat(WebApiConfig.XamlMappingValue,
                MediaTypeHeaderValue.Parse(WebApiConfig.XamlMediaType));
            formatterMappings.SetMediaTypeMappingForFormat(WebApiConfig.JsonDefinitionMappingValue,
                MediaTypeHeaderValue.Parse(WebApiConfig.JsonDefinitionMediaType));
            formatterMappings.SetMediaTypeMappingForFormat(WebApiConfig.JsonDefinition2MappingValue,
                MediaTypeHeaderValue.Parse(WebApiConfig.JsonDefinition2MediaType));
            formatterMappings.SetMediaTypeMappingForFormat(WebApiConfig.BinaryMappingValue,
                MediaTypeHeaderValue.Parse(WebApiConfig.BinaryMediaType));
        }
    }
}