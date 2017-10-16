using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Formatting;
using System.Web.Http;
using Microsoft.Practices.Unity;
using QA.Core.DPC.Formatters.Configuration;
using QA.Core.DPC.Formatters.Services;
using QA.Core.Models.Entities;
using QA.Core.DPC.Formatters.Formatting;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.WebApi.App_Start;
using QA.ProductCatalog.WebApi.Filters;
using QA.Core.Models.Configuration;

namespace QA.ProductCatalog.WebApi
{
    public static class WebApiConfig
    {
        #region Constants
        private const string XamlMappingValue = "xaml";
        private const string XmlMappingValue = "xml";
        private const string JsonMappingValue = "json";
        private const string PdfMappingValue = "pdf";
        private const string BinaryMappingValue = "binary";
        private const string XmlMediaType = "application/xml";
        private const string XamlMediaType = "application/xaml+xml";
        private const string JsonMediaType = "application/json";
        private const string PdfMediaType = "application/pdf";
        private const string BinaryMediaType = "application/octet-stream";
        private const string JsonDefinitionMappingValue = "jsonDefinition";
        private const string JsonDefinition2MappingValue = "jsonDefinition2";
        private static readonly string FormatConstraints = $"{XamlMappingValue}|{XmlMappingValue}|{JsonMappingValue}|{PdfMappingValue}|{BinaryMappingValue}|{JsonDefinitionMappingValue}|{JsonDefinitionMappingValue}";

        #endregion

        public static void Register(HttpConfiguration config)
        {
            #region IOC container
            var container = UnityConfig.Configure();
            config.DependencyResolver = new UnityResolver(container);
            //config.Services.Add(typeof(IExceptionLogger), new ExceptionLoggerAdapter());
            #endregion

            #region Routing
            var connection = container.Resolve<IConnectionProvider>();
            var customerCode = connection.QPMode ? "{customerCode}/" : string.Empty;

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
               name: "GetProduct",
               routeTemplate: $"api/{customerCode}{{version}}/{{slug}}/{{format}}/{{id}}",
               defaults: new { controller = "Product", action="GetProduct" },
               constraints: new { id = @"\d+", format = FormatConstraints }
           );

            config.Routes.MapHttpRoute(
              name: "PublishTarantoolProduct",
              routeTemplate: $"api/{customerCode}tarantool/publish/{{format}}",
              defaults: new { controller = "Product", action = "TarantoolPublish" },
              constraints: new { productId = @"\d+", format = FormatConstraints }
          );

            config.Routes.MapHttpRoute(
                name: "GetTarantoolProduct",
                routeTemplate: $"api/{customerCode}tarantool/{{format}}/{{productId}}",
                defaults: new { controller = "Product", action = "TarantoolGet" },
                constraints: new { productId = @"\d+", format = FormatConstraints }
            );           

            config.Routes.MapHttpRoute(
                name: "PostProduct",
                routeTemplate: $"api/{customerCode}{{version}}/{{slug}}/{{format}}/{{id}}",
                defaults: new { controller = "Product", action = "Post" },
                constraints: new { id = @"\d+", format = FormatConstraints }
            );

            config.Routes.MapHttpRoute(
                name: "CustomAction",
                routeTemplate: $"api/{customerCode}custom/{{format}}/{{name}}/{{id}}",
                defaults: new { controller = "Product", action = "CustomAction" },
                constraints: new { id = @"\d+", format = FormatConstraints }
            );

            config.Routes.MapHttpRoute(
                name: "DeleteProduct",
                routeTemplate: $"api/{customerCode}{{format}}/{{id}}",
                defaults: new { controller = "Product", action = "Delete" },
                constraints: new { id = @"\d+", format = FormatConstraints }
            );

            config.Routes.MapHttpRoute(
                name: "ListProduct",
                routeTemplate: $"api/{customerCode}{{version}}/{{slug}}/{{format}}",
                defaults: new { controller = "Product", action = "List", format = JsonMappingValue },
                constraints: new { format = FormatConstraints }
            );

            config.Routes.MapHttpRoute(
                name: "SearchProduct",
                routeTemplate: $"api/{customerCode}{{version}}/{{slug}}/search/{{format}}/{{query}}",
                defaults: new { controller = "Product", action = "Search" },
                constraints: new { format = FormatConstraints }
            );

            config.Routes.MapHttpRoute(
                name: "Schema",
                routeTemplate: $"api/{customerCode}{{version}}/{{slug}}/schema/{{format}}",
                defaults: new { controller = "Product", action = "Schema" },
                constraints: new { format = FormatConstraints }
            );

            #endregion

            #region Formatters
            config.Formatters.Clear();
            config.Formatters.AddModelMediaTypeFormatter<XamlSchemaFormatter, Content>(container, XamlMappingValue, XamlMediaType, RegisterMediaTypeMappings);
            config.Formatters.AddModelMediaTypeFormatter<XamlProductFormatter, Article>(container, XamlMappingValue, XamlMediaType, RegisterMediaTypeMappings);
            config.Formatters.AddModelMediaTypeFormatter<XmlSchemaFormatter, Content>(container, XmlMappingValue, XmlMediaType, RegisterMediaTypeMappings);
            config.Formatters.AddModelMediaTypeFormatter<XmlProductFormatter, Article>(container, XmlMappingValue, XmlMediaType, RegisterMediaTypeMappings);
            config.Formatters.AddModelMediaTypeFormatter<JsonSchemaFormatter, Content>(container, JsonMappingValue, JsonMediaType, RegisterMediaTypeMappings);
            config.Formatters.AddModelMediaTypeFormatter<JsonProductFormatter, Article>(container, JsonMappingValue, JsonMediaType, RegisterMediaTypeMappings);
            config.Formatters.AddModelMediaTypeFormatter<PdfProductFormatter, Article>(container, PdfMappingValue, PdfMediaType, RegisterMediaTypeMappings);
            config.Formatters.AddModelMediaTypeFormatter<BinaryModelFormatter<Content>, Content>(container, BinaryMappingValue, BinaryMediaType, RegisterMediaTypeMappings);
            config.Formatters.AddModelMediaTypeFormatter<BinaryModelFormatter<Article>, Article>(container, BinaryMappingValue, BinaryMediaType, RegisterMediaTypeMappings);
            config.Formatters.AddModelMediaTypeFormatter<BinaryModelFormatter<int[]>, int[]>(container, BinaryMappingValue, BinaryMediaType, RegisterMediaTypeMappings);
            config.Formatters.AddModelMediaTypeFormatter<BinaryModelFormatter<Dictionary<string, object>[]>, Dictionary<string, object>[]>(container, BinaryMappingValue, BinaryMediaType, RegisterMediaTypeMappings);
            config.Formatters.AddModelMediaTypeFormatter<BinaryModelFormatter<Dictionary<string, string>>, Dictionary<string, string>>(container, BinaryMappingValue, BinaryMediaType, RegisterMediaTypeMappings);
            config.Formatters.AddModelMediaTypeFormatter<JsonDefinitionSchemaFormatter, Content>(container, JsonDefinitionMappingValue, JsonMediaType, RegisterMediaTypeMappings);
            config.Formatters.AddModelMediaTypeFormatter<JsonDefinitionSchemaClassifiersAsBackwardsFormatter, Content>(container, JsonDefinition2MappingValue, JsonMediaType, RegisterMediaTypeMappings);

            var unsupportedMediatypeFormatter = new ResponseExceptionMediaTypeFormatter(HttpStatusCode.UnsupportedMediaType, PdfMediaType);
            config.Formatters.Add(unsupportedMediatypeFormatter);
            config.Formatters.Add(new XmlMediaTypeFormatter());
            config.Formatters.Add(new JsonMediaTypeFormatter());
            RegisterMediaTypeMappings(config.Formatters.JsonFormatter, JsonMappingValue, JsonMediaType);
            RegisterMediaTypeMappings(config.Formatters.XmlFormatter, XmlMappingValue, XmlMediaType);
            RegisterMediaTypeMappings(config.Formatters.JsonFormatter, JsonDefinitionMappingValue, JsonMediaType);
            RegisterMediaTypeMappings(unsupportedMediatypeFormatter, PdfMappingValue, PdfMediaType);
            #endregion

            #region Filters
            var exceptionFormatters = new MediaTypeFormatterCollection();
            exceptionFormatters.Clear();
            exceptionFormatters.AddModelMediaTypeFormatter<BinaryModelFormatter<Exception>, Exception>(container, BinaryMappingValue, BinaryMediaType, RegisterMediaTypeMappings);
            config.Filters.Add(new ExceptionFormatterFilterAttribute(exceptionFormatters));
            config.Filters.Add(container.Resolve<ExceptionLoggerFilterAttribute>());

            GlobalConfiguration.Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            #endregion

            MvcConfig.Init(container);
        }

        private static void RegisterMediaTypeMappings(MediaTypeFormatter formatter, string name, string mediaType)
        {
            formatter.AddRouteMapping("format", name, mediaType);
        }
    }
}
