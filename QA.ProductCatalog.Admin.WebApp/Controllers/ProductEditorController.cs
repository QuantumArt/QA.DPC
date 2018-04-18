using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using QA.ProductCatalog.Infrastructure;
using QA.Core.DPC.Formatters.Services;
using QA.Core.DPC.Loader;
using QA.ProductCatalog.Admin.WebApp.Models;
using QA.Core.Models.Configuration;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    public class ProductEditorController : Controller
    {
        private readonly IContentDefinitionService _contentDefinitionService;
        private readonly JsonProductService _jsonProductService;
        private readonly EditorSchemaFormatter _editorSchemaFormatter;
        
        public ProductEditorController(
            IContentDefinitionService contentDefinitionService,
            JsonProductService jsonProductService,
            EditorSchemaFormatter editorSchemaFormatter)
        {
            _contentDefinitionService = contentDefinitionService;
            _jsonProductService = jsonProductService;
            _editorSchemaFormatter = editorSchemaFormatter;
        }

        public ActionResult TypeScriptSchema(string slug, string version)
        {
            ServiceDefinition definition = _contentDefinitionService.GetServiceDefinition(slug, version, true);

            string jsonSchema = _jsonProductService.GetSchemaString(definition.Content, false);

            string editorSchema = _editorSchemaFormatter.GetSchema(definition.Content, false);

            return View(new ProductEditorSchemaModel
            {
                JsonSchema = jsonSchema,
                EditorSchema = editorSchema,
            });
        }

        public ActionResult PartialContentSchema(string slug, string version)
        {
            ServiceDefinition definition = _contentDefinitionService.GetServiceDefinition(slug, version, true);

            Content content = definition.Content.DeepCopy();

            string[] paths = new[]
            {
                //"/339",
                //"/339:1761/413",
                //"/339:1761/413:1760/361",
                //"/339:1326/290",
                //"/339:1326/290:1587/379",
                //"/339:1326/290:1587/379:1510/378",
                //"/339:1542/383",
                //"/339:1542/383:1540/402",
                //"/339:1542/383:1540/402",
                //"/339:1542/383:1540/402:2045/447"
                "/339:1326/290",
                "/339:1326/290:1587/379",
                "/339:1326/290:1587/379:1510/378",
            };

            Content partialContent = _editorSchemaFormatter.GetPartialContent(content, paths);

            string jsonSchema = _jsonProductService.GetSchemaString(partialContent, false);

            string editorSchema = _editorSchemaFormatter.GetSchema(partialContent, false);

            return View(nameof(TypeScriptSchema), new ProductEditorSchemaModel
            {
                JsonSchema = jsonSchema,
                EditorSchema = editorSchema,
            });
        }
    }
}
