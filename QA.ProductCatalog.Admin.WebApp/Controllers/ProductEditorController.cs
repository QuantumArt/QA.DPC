using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using QA.ProductCatalog.Infrastructure;
using QA.Core.DPC.Formatters.Services;
using QA.Core.DPC.Loader;
using QA.ProductCatalog.Admin.WebApp.Models;

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
    }
}
