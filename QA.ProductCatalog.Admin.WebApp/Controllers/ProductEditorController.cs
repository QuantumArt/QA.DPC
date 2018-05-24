using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Editor;
using QA.Core.DPC.Loader.Services;
using QA.Core.Models;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Admin.WebApp.Binders;
using QA.ProductCatalog.Admin.WebApp.Models;
using QA.ProductCatalog.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    [RoutePrefix("ProductEditor")]
    public class ProductEditorController : Controller
    {
        private readonly IContentDefinitionService _contentDefinitionService;
        private readonly IProductService _productService;
        private readonly IProductUpdateService _productUpdateService;
        private readonly JsonProductService _jsonProductService;
        private readonly IReadOnlyArticleService _articleService;
        private readonly EditorSchemaService _editorSchemaService;
        private readonly EditorDataService _editorDataService;
        private readonly EditorPartialContentService _editorPartialContentService;

        public ProductEditorController(
            IContentDefinitionService contentDefinitionService,
            IProductService productService,
            IProductUpdateService productUpdateService,
            JsonProductService jsonProductService,
            IReadOnlyArticleService articleService,
            EditorSchemaService editorSchemaService,
            EditorDataService editorDataService,
            EditorPartialContentService editorPartialContentService)
        {
            _contentDefinitionService = contentDefinitionService;
            _productService = productService;
            _productUpdateService = productUpdateService;
            _jsonProductService = jsonProductService;
            _articleService = articleService;
            _editorSchemaService = editorSchemaService;
            _editorDataService = editorDataService;
            _editorPartialContentService = editorPartialContentService;
        }

        [Route("{productDefinitionId}/{editorName}/{articleId}")]   
        public ActionResult Index(int productDefinitionId, string editorName, int articleId)
        {
            return View($"Editors/{editorName}");
        }
        
        /// <summary>
        /// Построить TypeScript-описание продукта.
        /// </summary>
        /// <param name="productDefinitionId">Id описания продукта</param>
        [HttpGet]
        public ViewResult TypeScriptSchema(int productDefinitionId, bool isLive = false)
        {
            Content content = GetContentByProductDefinitionId(productDefinitionId, isLive);

            string jsonSchema = _jsonProductService.GetEditorJsonSchemaString(content);

            return View(new ProductEditorSchemaModel
            {
                JsonSchema = jsonSchema,
            });
        }

        /// <summary>
        /// Построить TypeScript-описание схемы для редактора продукта.
        /// </summary>
        /// <param name="productDefinitionId">Id описания продукта</param>
        [HttpGet]
        public ViewResult ProductEditorSchema(int productDefinitionId, bool isLive = false)
        {
            Content content = GetContentByProductDefinitionId(productDefinitionId, isLive);
            
            ProductSchema productSchema = _editorSchemaService.GetProductSchema(content);

            Dictionary<int, ContentSchema> mergedSchema = _editorSchemaService.GetMergedContentSchemas(productSchema);

            string editorSchemaJson = JsonConvert.SerializeObject(productSchema, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                Converters = { new StringEnumConverter() }
            });
            
            string mergedSchemaJson = JsonConvert.SerializeObject(mergedSchema, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                Converters = { new StringEnumConverter() }
            });

            return View(new ProductEditorSchemaModel
            {
                EditorSchema = editorSchemaJson,
                MergedSchema = mergedSchemaJson,
            });
        }
        
        /// <summary>
        /// Построить JSON-схему части продукта, начиная с корневого контента,
        /// описанного путём <paramref name="contentPaths"/>[0].
        /// </summary>
        /// <param name="productDefinitionId">Id описания продукта</param>
        /// <param name="contentPaths">
        /// Массив путей контентов в продукте в формате <c>"/contentId:fieldId/.../contentId"</c>.
        /// Первый путь в массиве соответствует корневому контенту, остальные — его связям.
        /// Пример: <c>[ "/339:1326/290", "/339:1326/290:1587/379" ]</c>.
        /// </param>
        /// <returns>JSON-схема продукта</returns>
        [HttpPost]
        public ContentResult PartialJsonSchema_Test(
            int productDefinitionId,
            [ModelBinder(typeof(JsonModelBinder))] string[] contentPaths,
            bool isLive = false)
        {
            Content content = GetContentByProductDefinitionId(productDefinitionId, isLive);

            Content partialContent = _editorPartialContentService.GetPartialContent(content, contentPaths);

            string jsonSchema = _jsonProductService.GetEditorJsonSchemaString(partialContent);

            return Content(jsonSchema, "application/json");
        }

        /// <summary>
        /// Получить JSON схему для редактора продукта.
        /// </summary>
        /// <param name="productDefinitionId">Id описания продукта</param>
        /// <returns>JSON схемы редактора продукта</returns>
        [HttpGet]
        public ContentResult GetProductSchema(int productDefinitionId, bool isLive = false)
        {
            Content content = GetContentByProductDefinitionId(productDefinitionId, isLive);

            ProductSchema productSchema = _editorSchemaService.GetProductSchema(content);

            Dictionary<int, ContentSchema> mergedSchemas = _editorSchemaService.GetMergedContentSchemas(productSchema);

            var schemaModel = new
            {
                EditorSchema = productSchema,
                MergedSchemas = mergedSchemas,
            };

            string schemaJson = JsonConvert.SerializeObject(schemaModel, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                Converters = { new StringEnumConverter() }
            });

            return Content(schemaJson, "application/json");
        }

        private static readonly ConcurrentDictionary<int, ContentResult> _schemaCache
            = new ConcurrentDictionary<int, ContentResult>();

        [HttpGet]
        public ContentResult GetProductSchema_Test(int productDefinitionId, bool refresh = false)
        {
            if (refresh)
            {
                _schemaCache.TryRemove(productDefinitionId, out var _);
            }
            return _schemaCache.GetOrAdd(productDefinitionId, _ => GetProductSchema(productDefinitionId));
        }

        /// <summary>
        /// Получить JSON продукта по его <see cref="Article.Id"/>.
        /// </summary>
        /// <param name="articleId">Id корневой статьи</param>
        /// <returns>JSON продукта</returns>
        [HttpGet]
        public ContentResult GetProduct(int articleId, bool isLive = false)
        {
            Content content = GetContentByArticleId(articleId, isLive);

            var productDefinition = new ProductDefinition { StorageSchema = content };

            Article article = _productService.GetProductById(articleId, isLive, productDefinition);

            IArticleFilter filter = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;

            ContentObject data = _editorDataService.ConvertArticle(article, filter);

            string json = JsonConvert.SerializeObject(data);

            return Content(json, "application/json");
        }

        private static readonly ConcurrentDictionary<int, ContentResult> _dataCache
            = new ConcurrentDictionary<int, ContentResult>();

        [HttpGet]
        public ContentResult GetProduct_Test(int articleId, bool refresh = false)
        {
            if (refresh)
            {
                _dataCache.TryRemove(articleId, out var _);
            }
            return _dataCache.GetOrAdd(articleId, _ => GetProduct(articleId));
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

            Content content = GetContentByProductDefinitionId(request.ProductDefinitionId, request.IsLive);

            Content partialContent = _editorPartialContentService.GetPartialContent(content, request.ContentPaths);

            // TODO: deserialize by _editorProductService
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
            /// Id описания продукта
            /// </summary>
            public int ProductDefinitionId { get; set; }
            
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

        private Content GetContentByProductDefinitionId(int productDefinitionId, bool isLive)
        {
            Content content = _contentDefinitionService
                .GetDefinitionById(productDefinitionId, isLive)
                .DeepCopy();

            return content;
        }

        private Content GetContentByArticleId(int articleId, bool isLive)
        {
            _articleService.IsLive = isLive;

            var qpArticle = _articleService.Read(articleId);

            string productTypeField = qpArticle.FieldValues
                .Where(x => x.Field.IsClassifier)
                .Select(x => x.Value)
                .FirstOrDefault();

            int productTypeId = Int32.Parse(productTypeField);

            Content content = _contentDefinitionService
                .GetDefinitionForContent(productTypeId, qpArticle.ContentId)
                .DeepCopy();

            return content;
        }

        [HttpGet]
        public ViewResult ComponentLibrary()
        {
            return View();
        }
    }
}