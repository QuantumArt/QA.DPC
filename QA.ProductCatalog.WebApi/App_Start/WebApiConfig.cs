﻿//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Net.Http.Formatting;
//using System.Web.Http;
//using Unity;
//using QA.Core.DPC.Formatters.Configuration;
//using QA.Core.DPC.Formatters.Services;
//using QA.Core.Models.Entities;
//using QA.Core.DPC.Formatters.Formatting;
//using QA.Core.DPC.QP.Services;
//using QA.ProductCatalog.WebApi.App_Start;
//using QA.ProductCatalog.WebApi.Filters;
//using QA.Core.Models.Configuration;
//using QA.Core.DPC.QP.Autopublish.Models;
//using System.Web.Http.Routing;

namespace QA.ProductCatalog.WebApi
{
    public static class WebApiConfig
    {
        #region Constants
        public const string XamlMappingValue = "xaml";
        public const string XmlMappingValue = "xml";
        public const string JsonMappingValue = "json";
        public const string PdfMappingValue = "pdf";
        public const string BinaryMappingValue = "binary";
        public const string JsonDefinitionMappingValue = "jsonDefinition";
        public const string JsonDefinition2MappingValue = "jsonDefinition2";
        public const string XmlMediaType = "application/xml";
        public const string XamlMediaType = "application/xaml+xml";
        public const string JsonMediaType = "application/json";
        public const string JsonDefinitionMediaType = "application/x-definition+json";
        public const string JsonDefinition2MediaType = "application/x-definition2+json";
        public const string PdfMediaType = "application/pdf";
        public const string BinaryMediaType = "application/octet-stream";
        public static readonly string[] MappingsValues = new[] { XamlMappingValue, XmlMappingValue, JsonMappingValue, PdfMappingValue, BinaryMappingValue, JsonDefinitionMappingValue, JsonDefinition2MappingValue };
        #endregion

//        public static void Register(HttpConfiguration config)
//        {
//
//            #region Routing
//
//            var constraintResolver = new DefaultInlineConstraintResolver();
//            constraintResolver.ConstraintMap.Add("media_type_mapping", typeof(MediaTypeMappingConstraint));
//
//            config.MapHttpAttributeRoutes(constraintResolver);
//            #endregion
//
//            #region Formatters
//            config.Formatters.Clear();
//            config.Formatters.AddModelMediaTypeFormatter<XamlSchemaFormatter, Content>(container, XamlMappingValue, XamlMediaType, RegisterMediaTypeMappings);
//            config.Formatters.AddModelMediaTypeFormatter<XamlProductFormatter, Article>(container, XamlMappingValue, XamlMediaType, RegisterMediaTypeMappings);
//            config.Formatters.AddModelMediaTypeFormatter<XmlSchemaFormatter, Content>(container, XmlMappingValue, XmlMediaType, RegisterMediaTypeMappings);
//            config.Formatters.AddModelMediaTypeFormatter<XmlProductFormatter, Article>(container, XmlMappingValue, XmlMediaType, RegisterMediaTypeMappings);
//            config.Formatters.AddModelMediaTypeFormatter<JsonSchemaFormatter, Content>(container, JsonMappingValue, JsonMediaType, RegisterMediaTypeMappings);
//            config.Formatters.AddModelMediaTypeFormatter<JsonProductFormatter, Article>(container, JsonMappingValue, JsonMediaType, RegisterMediaTypeMappings);
//            config.Formatters.AddModelMediaTypeFormatter<PdfProductFormatter, Article>(container, PdfMappingValue, PdfMediaType, RegisterMediaTypeMappings);
//            config.Formatters.AddModelMediaTypeFormatter<BinaryModelFormatter<ProductItem>, ProductItem>(container, BinaryMappingValue, BinaryMediaType, RegisterMediaTypeMappings);
//            config.Formatters.AddModelMediaTypeFormatter<BinaryModelFormatter<Content>, Content>(container, BinaryMappingValue, BinaryMediaType, RegisterMediaTypeMappings);
//            config.Formatters.AddModelMediaTypeFormatter<BinaryModelFormatter<Article>, Article>(container, BinaryMappingValue, BinaryMediaType, RegisterMediaTypeMappings);
//            config.Formatters.AddModelMediaTypeFormatter<BinaryModelFormatter<int[]>, int[]>(container, BinaryMappingValue, BinaryMediaType, RegisterMediaTypeMappings);
//            config.Formatters.AddModelMediaTypeFormatter<BinaryModelFormatter<Dictionary<string, object>[]>, Dictionary<string, object>[]>(container, BinaryMappingValue, BinaryMediaType, RegisterMediaTypeMappings);
//            config.Formatters.AddModelMediaTypeFormatter<BinaryModelFormatter<Dictionary<string, string>>, Dictionary<string, string>>(container, BinaryMappingValue, BinaryMediaType, RegisterMediaTypeMappings);
//            config.Formatters.AddModelMediaTypeFormatter<JsonDefinitionSchemaFormatter, Content>(container, JsonDefinitionMappingValue, JsonMediaType, RegisterMediaTypeMappings);
//            config.Formatters.AddModelMediaTypeFormatter<JsonDefinitionSchemaClassifiersAsBackwardsFormatter, Content>(container, JsonDefinition2MappingValue, JsonMediaType, RegisterMediaTypeMappings);
//
//            config.Formatters.AddModelMediaTypeFormatter<JsonProductArrayFormatter, IEnumerable<Article>>(container, JsonMappingValue, JsonMediaType, RegisterMediaTypeMappings);
//
//            var unsupportedMediatypeFormatter = new ResponseExceptionMediaTypeFormatter(HttpStatusCode.UnsupportedMediaType, PdfMediaType);
//            config.Formatters.Add(unsupportedMediatypeFormatter);
//            config.Formatters.Add(new XmlMediaTypeFormatter());
//            config.Formatters.Add(new JsonMediaTypeFormatter());
//            RegisterMediaTypeMappings(config.Formatters.JsonFormatter, JsonMappingValue, JsonMediaType);
//            RegisterMediaTypeMappings(config.Formatters.XmlFormatter, XmlMappingValue, XmlMediaType);
//            RegisterMediaTypeMappings(config.Formatters.JsonFormatter, JsonDefinitionMappingValue, JsonMediaType);
//            RegisterMediaTypeMappings(unsupportedMediatypeFormatter, PdfMappingValue, PdfMediaType);
//            #endregion
//
//            #region Filters
//            var exceptionFormatters = new MediaTypeFormatterCollection();
//            exceptionFormatters.Clear();
//            exceptionFormatters.AddModelMediaTypeFormatter<BinaryModelFormatter<Exception>, Exception>(container, BinaryMappingValue, BinaryMediaType, RegisterMediaTypeMappings);
//            config.Filters.Add(new ExceptionFormatterFilterAttribute(exceptionFormatters));
//            config.Filters.Add(container.Resolve<ExceptionLoggerFilterAttribute>());
//
//            GlobalConfiguration.Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
//
//            #endregion
//        }
//
//        private static void RegisterMediaTypeMappings(MediaTypeFormatter formatter, string name, string mediaType)
//        {
//            formatter.AddRouteMapping("format", name, mediaType);
//        }
    }
}
