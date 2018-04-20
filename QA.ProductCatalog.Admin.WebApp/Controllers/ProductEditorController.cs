using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Core.DPC.Formatters.Services;
using QA.Core.DPC.Loader;
using QA.Core.Models;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Admin.WebApp.Binders;
using QA.ProductCatalog.Admin.WebApp.Models;
using QA.ProductCatalog.Infrastructure;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Web.Mvc;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    public class ProductEditorController : Controller
    {
        private readonly IContentDefinitionService _contentDefinitionService;
        private readonly IProductService _productService;
        private readonly IProductUpdateService _productUpdateService;
        private readonly JsonProductService _jsonProductService;
        private readonly EditorSchemaFormatter _editorSchemaFormatter;

        public ProductEditorController(
            IContentDefinitionService contentDefinitionService,
            IProductService productService,
            IProductUpdateService productUpdateService,
            JsonProductService jsonProductService,
            EditorSchemaFormatter editorSchemaFormatter)
        {
            _contentDefinitionService = contentDefinitionService;
            _productService = productService;
            _productUpdateService = productUpdateService;
            _jsonProductService = jsonProductService;
            _editorSchemaFormatter = editorSchemaFormatter;
        }

        /// <summary>
        /// Построить TypeScript-описание продукта и схему для редактора.
        /// </summary>
        /// <param name="slug">Имя продукта</param>
        /// <param name="version">Версия продукта</param>
        [HttpGet]
        public ActionResult TypeScriptSchema(string slug, string version)
        {
            Content content = _contentDefinitionService.GetServiceDefinition(slug, version).Content;

            string jsonSchema = _jsonProductService.GetSchemaString(content, false);

            string editorSchema = _editorSchemaFormatter.GetSchema(content, false);

            return View(new ProductEditorSchemaModel
            {
                JsonSchema = jsonSchema,
                EditorSchema = editorSchema,
            });
        }

        /// <summary>
        /// Построить JSON-схему части продукта, начиная с корневого контента,
        /// описанного путём <paramref name="contentPaths"/>[0].
        /// </summary>
        /// <param name="slug">Имя продукта</param>
        /// <param name="version">Версия продукта</param>
        /// <param name="contentPaths">
        /// Массив путей контентов в продукте в формате <c>/contentId:fieldId/.../contentId</c>.
        /// Первый путь в массиве соответствует корневому контенту, остальные — его связям.
        /// Пример: <c>[ "/339:1326/290", "/339:1326/290:1587/379" ]</c>.
        /// </param>
        /// <returns>JSON-схема продукта</returns>
        [HttpPost]
        public ActionResult PartialJsonSchema(
            string slug, string version, [ModelBinder(typeof(JsonModelBinder))] string[] contentPaths)
        {
            Content content = _contentDefinitionService.GetServiceDefinition(slug, version).Content;

            Content partialContent = _editorSchemaFormatter.GetPartialContent(content.DeepCopy(), contentPaths);

            string jsonSchema = _jsonProductService.GetSchemaString(partialContent, false);

            return Content(jsonSchema, "application/json");
        }

        /// <summary>
        /// Получить JSON продукта по его <see cref="Article.Id"/>.
        /// </summary>
        /// <param name="slug">Имя продукта</param>
        /// <param name="version">Версия продукта</param>
        /// <param name="id"><see cref="Article.Id"/></param>
        /// <param name="isLive"></param>
        /// <returns>JSON продукта</returns>
        [HttpGet]
        public ActionResult GetProduct(string slug, string version, int id, bool isLive = false)
        {
            Content content = _contentDefinitionService.GetServiceDefinition(slug, version).Content;

            var productDefinition = new ProductDefinition { StorageSchema = content };

            Article article = _productService.GetProductById(id, isLive, productDefinition);

            IArticleFilter filter = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;

            Dictionary<string, object> data = _jsonProductService.ConvertArticle(article, filter);

            string json = JsonConvert.SerializeObject(data);

            return Content(json, "application/json");
        }

        /// <summary>
        /// Сохранить часть продукта начиная с корневого контента,
        /// описанного путём <see cref="PartialProductRequest.ContentPaths"/>[0].
        /// </summary>
        [HttpPost]
        public ActionResult SavePartialProduct(
            [ModelBinder(typeof(JsonModelBinder))] PartialProductRequest request)
        {
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Content content = _contentDefinitionService.GetServiceDefinition(request.Slug, request.Version).Content;

            Content partialContent = _editorSchemaFormatter.GetPartialContent(content, request.ContentPaths);

            Article partialProduct = _jsonProductService.DeserializeProduct(request.PartialProduct, partialContent);

            var partialDefinition = new ProductDefinition { StorageSchema = partialContent };

            // TODO: what about validation ?
            // TODO: what about Id-s of new articles ?
            // _productUpdateService.Update(partialProduct, partialDefinition, request.IsLive);

            return new HttpStatusCodeResult(HttpStatusCode.NoContent);
        }
        
        public class PartialProductRequest
        {
            /// <summary>
            /// Имя продукта
            /// </summary>
            [Required]
            public string Slug { get; set; }

            /// <summary>
            /// Версия продукта
            /// </summary>
            [Required]
            public string Version { get; set; }

            /// <summary>
            /// Массив путей контентов в продукте в формате <c>/contentId:fieldId/.../contentId</c>.
            /// Первый путь в массиве соответствует корневому контенту, остальные — его связям.
            /// Пример: <c>[ "/339:1326/290", "/339:1326/290:1587/379" ]</c>.
            /// </summary>
            [Required]
            public string[] ContentPaths { get; set; }

            /// <summary>
            /// Фильтрация по IsLive
            /// </summary>
            public bool IsLive { get; set; }

            /// <summary>
            /// JSON части продукта, начиная с корневого контента, описанного путём <see cref="ContentPaths"/>[0]
            /// </summary>
            [Required]
            public JObject PartialProduct { get; set; }
        }
    }
}