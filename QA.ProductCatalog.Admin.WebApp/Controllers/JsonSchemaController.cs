using QA.Core.DPC.Loader;
using QA.Core.Models.Configuration;
using QA.ProductCatalog.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using QA.ProductCatalog.Admin.WebApp.Filters;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
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
        
        [HttpGet, RequireCustomAction]
        public ActionResult TypeScriptSchema(int content_item_id, bool isLive = false)
        {
            Content rootContent = _contentDefinitionService.GetDefinitionById(content_item_id, isLive);

            string jsonSchema = _jsonProductService.GetEditorJsonSchemaString(rootContent);

            return View((object)jsonSchema);
        }
    }
}