using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using QA.Core.DPC.Formatters.Formatting;
using QA.Core.DPC.Formatters.Services;
using QA.Core.DPC.Loader;
using QA.Core.DPC.QP.Models;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.DPC.Core.Helpers;
using QA.ProductCatalog.WebApi.App_Start;
using QA.ProductCatalog.WebApi.Filters;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            services.AddHttpClient();

            services.AddTransient<IActionContextAccessor, ActionContextAccessor>();
            
            services.Configure<ConnectionProperties>(Configuration.GetSection("Connection"));
            services.Configure<LoaderProperties>(Configuration.GetSection("Loader"));
            services.Configure<IntegrationProperties>(Configuration.GetSection("Integration"));
            services.Configure<Properties>(Configuration.GetSection("Properties"));                 
            services.Configure<AuthProperties>(Configuration.GetSection("Properties"));            
            
            services
                .AddMvc(options => {             
                    options.Filters.Add(typeof(GlobalExceptionFilterAttribute));
                    options.EnableEndpointRouting = false;
                    RegisterMediaTypes(options.FormatterMappings);
                    RegisterOutputFormatters(options.OutputFormatters);
                    RegisterInputFormatters(options.InputFormatters); })
                .AddXmlSerializerFormatters().AddControllersAsServices();
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "DPC Web API", 
                    Version = "v1",
                    Description = "This API allows to manipulate products: get schemas, perform CRUD operations"
                });
                
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });         
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DPC Web API");
            });

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
        
        
        private static void RegisterInputFormatters(FormatterCollection<IInputFormatter> formatters)
        {
            formatters.Insert(0,
                new ModelMediaTypeInputFormatter<Article, JsonProductFormatter>(WebApiConstants.JsonMediaType));
            formatters.Insert(0,
                new ModelMediaTypeInputFormatter<Article, XmlProductFormatter>(WebApiConstants.XmlMediaType));
            formatters.Insert(0,
                new ModelMediaTypeInputFormatter<Article, XamlProductFormatter>(WebApiConstants.XamlMediaType));

                            
            formatters.Add(new BinaryInputFormatter(WebApiConstants.BinaryMediaType));

        }

        private static void RegisterOutputFormatters(FormatterCollection<IOutputFormatter> formatters)
        {
            var jsonOutputFormatter = RemoveDefaultJsonOutputFormatter(formatters);

            RegisterArticleOutputFormatters(formatters);

            RegisterContentOutputFormatters(formatters);

            RegisterAuxOutputFormatters(formatters);

            RestoreDefaultJsonOutputFormatter(formatters, jsonOutputFormatter);
        }

        private static void RestoreDefaultJsonOutputFormatter(FormatterCollection<IOutputFormatter> formatters, SystemTextJsonOutputFormatter jsonOutputFormatter)
        {
            if (jsonOutputFormatter != null)
            {
                formatters.Add(jsonOutputFormatter);
            }
        }

        private static SystemTextJsonOutputFormatter RemoveDefaultJsonOutputFormatter(FormatterCollection<IOutputFormatter> formatters)
        {
            var jsonOutputFormatter = formatters.OfType<SystemTextJsonOutputFormatter>().FirstOrDefault();
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
                >(WebApiConstants.XmlMediaType)
            );

            formatters.Add(
                new ModelMediaTypeOutputFormatter<
                    IEnumerable<Article>, JsonProductArrayFormatter
                >(WebApiConstants.JsonMediaType)
            );
            
            formatters.Add(
                new ModelMediaTypeOutputFormatter<
                    IEnumerable<Article>, XmlDataContractFormatter<IEnumerable<Article>>
                >(WebApiConstants.XmlMediaType)
            );
            
            formatters.Add(new BinaryOutputFormatter(WebApiConstants.BinaryMediaType));
        }

        private static void RegisterContentOutputFormatters(FormatterCollection<IOutputFormatter> formatters)
        {
            formatters.Add(
                new ModelMediaTypeOutputFormatter<Content, XamlSchemaFormatter>(WebApiConstants.XamlMediaType));
            formatters.Add(
                new ModelMediaTypeOutputFormatter<Content, XmlSchemaFormatter>(WebApiConstants.XmlMediaType));
            formatters.Add(
                new ModelMediaTypeOutputFormatter<Content, JsonSchemaFormatter>(WebApiConstants.JsonMediaType));
            formatters.Add(
                new ModelMediaTypeOutputFormatter<Content, JsonDefinitionSchemaFormatter>(
                    WebApiConstants.JsonDefinitionMediaType)
            );
            formatters.Add(
                new ModelMediaTypeOutputFormatter<Content, JsonDefinitionSchemaClassifiersAsBackwardsFormatter>(
                    WebApiConstants.JsonDefinition2MediaType)
            );
        }

        private static void RegisterArticleOutputFormatters(FormatterCollection<IOutputFormatter> formatters)
        {
            formatters.Add(
                new ModelMediaTypeOutputFormatter<Article, XmlProductFormatter>(WebApiConstants.XmlMediaType));
            formatters.Add(
                new ModelMediaTypeOutputFormatter<Article, XamlProductFormatter>(WebApiConstants.XamlMediaType));
            formatters.Add(
                new ModelMediaTypeOutputFormatter<Article, JsonProductFormatter>(WebApiConstants.JsonMediaType));
#if !NETSTANDARD
            formatters.Add(
                new ModelMediaTypeOutputFormatter<Article, PdfProductFormatter>(WebApiConstants.PdfMediaType));
#endif
        }

        private static void RegisterMediaTypes(FormatterMappings formatterMappings)
        {
            formatterMappings.SetMediaTypeMappingForFormat(WebApiConstants.XmlMappingValue,
                MediaTypeHeaderValue.Parse(WebApiConstants.XmlMediaType));
            formatterMappings.SetMediaTypeMappingForFormat(WebApiConstants.JsonMappingValue,
                MediaTypeHeaderValue.Parse(WebApiConstants.JsonMediaType));
            formatterMappings.SetMediaTypeMappingForFormat(WebApiConstants.PdfMappingValue,
                MediaTypeHeaderValue.Parse(WebApiConstants.PdfMediaType));
            formatterMappings.SetMediaTypeMappingForFormat(WebApiConstants.XamlMappingValue,
                MediaTypeHeaderValue.Parse(WebApiConstants.XamlMediaType));
            formatterMappings.SetMediaTypeMappingForFormat(WebApiConstants.JsonDefinitionMappingValue,
                MediaTypeHeaderValue.Parse(WebApiConstants.JsonDefinitionMediaType));
            formatterMappings.SetMediaTypeMappingForFormat(WebApiConstants.JsonDefinition2MappingValue,
                MediaTypeHeaderValue.Parse(WebApiConstants.JsonDefinition2MediaType));
            formatterMappings.SetMediaTypeMappingForFormat(WebApiConstants.BinaryMappingValue,
                MediaTypeHeaderValue.Parse(WebApiConstants.BinaryMediaType));
        }
    }
}