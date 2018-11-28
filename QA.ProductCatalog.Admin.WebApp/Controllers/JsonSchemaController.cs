﻿using QA.Core.DPC.Loader;
using QA.Core.Models.Configuration;
using QA.ProductCatalog.Infrastructure;
using System.Web.Mvc;
using QA.Core.Web;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    [RoutePrefix("JsonSchema")]
    public class JsonSchemaController : Controller
    {
        private readonly IContentDefinitionService _contentDefinitionService;
        private readonly JsonProductService _jsonProductService;

        public JsonSchemaController(
            IContentDefinitionService contentDefinitionService,
            JsonProductService jsonProductService)
        {
            _contentDefinitionService = contentDefinitionService;
            _jsonProductService = jsonProductService;
        }
        
        /// <summary>
        /// Построить TypeScript-описание продукта для DPC API.
        /// </summary>
        /// <param name="content_item_id">Id описания продукта</param>
        [HttpGet, RequireCustomAction]
        public ViewResult TypeScriptSchema(int content_item_id, bool isLive = false)
        {
            Content rootContent = _contentDefinitionService.GetDefinitionById(content_item_id, isLive);

            string jsonSchema = _jsonProductService.GetEditorJsonSchemaString(rootContent);

            return View((object)jsonSchema);
        }
    }
}