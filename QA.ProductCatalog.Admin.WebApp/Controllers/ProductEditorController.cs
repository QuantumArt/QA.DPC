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
using QA.Core.Web;
using Quantumart.QP8.BLL.Services.API.Models;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    [RoutePrefix("ProductEditor")]
    public class ProductEditorController : Controller
    {
        private const string FIELD_NAME_EDITOR_VIEW_PATH = "EditorViewPath";
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

        [HttpGet]
        public ViewResult ComponentLibrary()
        {
            return View();
        }

        /// <summary>
        /// CustomAction для создания нового продукта
        /// </summary>
        /// <param name="content_item_id">Id описания продукта</param>
        [HttpGet, RequireCustomAction]
        public ViewResult Create(int content_item_id, bool isLive = false)
        {
            var fieldsByName = _contentDefinitionService.GetDefinitionFields(content_item_id, isLive);

            if (fieldsByName == null)
            {
                throw new InvalidOperationException($"ProductDefinition {content_item_id} was not found");
            }

            fieldsByName.TryGetValue(FIELD_NAME_EDITOR_VIEW_PATH, out string editorViewPath);

            if (String.IsNullOrWhiteSpace(editorViewPath))
            {
                editorViewPath = "DefaultEditor";
            }

            return View(editorViewPath, new ProductEditorSettingsModel
            {
                ProductDefinitionId = content_item_id,
            });
        }

        /// <summary>
        /// CustomAction для редактирования существующего продукта
        /// </summary>
        /// <param name="content_item_id">Id корневой статьи</param>
        [HttpGet, RequireCustomAction]
        public ViewResult Edit(int content_item_id, bool isLive = false)
        {
            var fieldsByName = GetDefinitionFieldsByArticleId(content_item_id, isLive);

            if (fieldsByName == null
                || !fieldsByName.TryGetValue(nameof(Article.Id), out string productDefinitionsId))
            {
                throw new InvalidOperationException($"ProductDefinition for article {content_item_id} was not found");
            }

            fieldsByName.TryGetValue(FIELD_NAME_EDITOR_VIEW_PATH, out string editorViewPath);

            if (String.IsNullOrWhiteSpace(editorViewPath))
            {
                editorViewPath = "DefaultEditor";
            }

            return View(editorViewPath, new ProductEditorSettingsModel
            {
                ArticleId = content_item_id,
                ProductDefinitionId = Int32.Parse(productDefinitionsId),
            });
        }

        /// <summary>
        /// Построить TypeScript-описание продукта.
        /// </summary>
        /// <param name="content_item_id">Id описания продукта</param>
        [HttpGet, RequireCustomAction]
        public ViewResult TypeScriptSchema(int content_item_id, bool isLive = false)
        {
            Content content = _contentDefinitionService.GetDefinitionById(content_item_id, isLive);

            string jsonSchema = _jsonProductService.GetEditorJsonSchemaString(content);

            return View(new ProductEditorSchemaModel
            {
                JsonSchema = jsonSchema,
            });
        }

        /// <summary>
        /// Построить TypeScript-описание схемы для редактора продукта.
        /// </summary>
        /// <param name="content_item_id">Id описания продукта</param>
        [HttpGet, RequireCustomAction]
        public ViewResult ProductEditorSchema(int content_item_id, bool isLive = false)
        {
            Content content = _contentDefinitionService.GetDefinitionById(content_item_id, isLive);

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
        /// описанного путём <paramref name="contentPath"/>
        /// и поддеревом выбора частичного продукта <see cref="contentSelection"/>.
        /// </summary>
        /// <param name="productDefinitionId">Id описания продукта</param>
        /// <param name="contentPath">Путь к контенту в продукте</param>
        /// <param name="relationSelection">Дерево выбора частичного продукта</param>
        /// <returns>JSON-схема продукта</returns>
        [HttpPost]
        public ContentResult PartialJsonSchema_Test(
            int productDefinitionId,
            string contentPath,
            [ModelBinder(typeof(JsonModelBinder))]
            RelationSelection relationSelection,
            bool isLive = false)
        {
            Content content = _contentDefinitionService.GetDefinitionById(productDefinitionId, isLive);

            Content partialContent = _editorPartialContentService
              .GetPartialContent(content, contentPath, relationSelection);

            string jsonSchema = _jsonProductService.GetEditorJsonSchemaString(partialContent);

            return Content(jsonSchema, "application/json");
        }

        /// <summary>
        /// Получить JSON схему для редактора продукта.
        /// </summary>
        /// <param name="productDefinitionId">Id описания продукта</param>
        /// <returns>JSON схемы редактора продукта</returns>
        [HttpGet]
        public ContentResult GetEditorSchema(int productDefinitionId, bool isLive = false)
        {
            Content content = _contentDefinitionService.GetDefinitionById(productDefinitionId, isLive);

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
        public ContentResult GetEditorSchema_Test(int productDefinitionId, bool refresh = false)
        {
            if (refresh)
            {
                _schemaCache.TryRemove(productDefinitionId, out var _);
            }
            return _schemaCache.GetOrAdd(productDefinitionId, _ => GetEditorSchema(productDefinitionId));
        }

        /// <summary>
        /// Получить JSON продукта по его <see cref="Article.Id"/>.
        /// </summary>
        /// <param name="articleId">Id корневой статьи</param>
        /// <returns>JSON продукта</returns>
        [HttpGet]
        public ContentResult GetEditorData(int articleId, bool isLive = false)
        {
            Content content = GetContentByArticleId(articleId, isLive);

            var productDefinition = new ProductDefinition { StorageSchema = content };

            Article article = _productService.GetProductById(articleId, isLive, productDefinition);

            IArticleFilter filter = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;

            ArticleObject articleObject = _editorDataService.ConvertArticle(article, filter);

            string json = JsonConvert.SerializeObject(articleObject);

            return Content(json, "application/json");
        }

        private static readonly ConcurrentDictionary<int, ContentResult> _dataCache
            = new ConcurrentDictionary<int, ContentResult>();

        [HttpGet]
        public ContentResult GetEditorData_Test(int articleId, bool refresh = false)
        {
            if (refresh)
            {
                _dataCache.TryRemove(articleId, out var _);
            }
            return _dataCache.GetOrAdd(articleId, _ => GetEditorData(articleId));
        }

        /// <summary>
        /// Загрузить часть продукта начиная с корневого контента,
        /// описанного путём <see cref="PartialProductRequest.ContentPath"/>
        /// игнорируя уже загруженные статьи <see cref="LoadPartialProductRequest.IgnoredArticleIdsByContent"/>
        /// </summary>
        /// <returns>JSON части продукта</returns>
        [HttpPost]
        public ActionResult LoadPartialProduct(
             [ModelBinder(typeof(JsonModelBinder))] LoadPartialProductRequest request, bool isLive = false)
        {
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Content rootContent = _contentDefinitionService
                .GetDefinitionById(request.ProductDefinitionId, isLive);

            Content partialContent = _editorPartialContentService
                .GetPartialContent(rootContent, request.ContentPath, request.RelationSelection);

            Article[] articles = _productService.GetProductsByIds(
                partialContent, request.ArticleIds, request.IgnoredArticleIdsByContent, isLive);
            
            IArticleFilter filter = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;

            ArticleObject[] articleObjects = articles
                .Select(article => _editorDataService.ConvertArticle(article, filter))
                .ToArray();

            string json = JsonConvert.SerializeObject(articleObjects);

            return Content(json, "application/json");
        }
        
        /// <summary>
        /// Сохранить часть продукта начиная с корневого контента,
        /// описанного путём <see cref="PartialProductRequest.ContentPath"/>
        /// и поддеревом выбора частичного продукта <see cref="PartialProductRequest.RelationSelection"/>.
        /// </summary>
        [HttpPost]
        public ActionResult SavePartialProduct(
            [ModelBinder(typeof(JsonModelBinder))] SavePartialProductRequest request, bool isLive = false)
        {
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Content rootContent = _contentDefinitionService
                .GetDefinitionById(request.ProductDefinitionId, isLive);

            Content partialContent = _editorPartialContentService
                .GetPartialContent(rootContent, request.ContentPath, request.RelationSelection);

            Article partialProduct = _editorDataService
                .DeserializeProduct(request.PartialProduct, partialContent);

            var partialDefinition = new ProductDefinition { StorageSchema = partialContent };

            // TODO: what about validation ?
            // TODO: concurrency checks based on `Modified` field
            InsertData[] idMappings = _productUpdateService.Update(partialProduct, partialDefinition, isLive);

            var idMappingsByContent = idMappings
                .ToLookup(data => data.ContentId, data => new
                {
                    ClientId = data.OriginalArticleId,
                    ServerId = data.CreatedArticleId,
                });

            string json = JsonConvert.SerializeObject(idMappingsByContent);

            return Content(json, "application/json");
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

            return _contentDefinitionService
                .GetDefinitionForContent(productTypeId, qpArticle.ContentId, isLive);
        }
        
        private IReadOnlyDictionary<string, string> GetDefinitionFieldsByArticleId(int articleId, bool isLive)
        {
            _articleService.IsLive = isLive;

            var qpArticle = _articleService.Read(articleId);

            string productTypeField = qpArticle.FieldValues
                .Where(x => x.Field.IsClassifier)
                .Select(x => x.Value)
                .FirstOrDefault();

            int productTypeId = Int32.Parse(productTypeField);

            return _contentDefinitionService
                .GetDefinitionFields(productTypeId, qpArticle.ContentId, isLive);
        }

        #region Requests

        public abstract class PartialProductRequest
        {
            /// <summary>
            /// Id описания продукта
            /// </summary>
            public int ProductDefinitionId { get; set; }

            /// <summary>
            /// Путь к контенту в продукте в формате <c>"/FieldName/.../ExtensionContentName/.../FieldName"</c>
            /// </summary>
            public string ContentPath { get; set; } = "/";

            /// <summary>
            /// Дерево выбора частичного продукта.
            /// Если <code>null</code> — то выбирается весь подграф продукта целиком.
            /// </summary>
            public RelationSelection RelationSelection { get; set; }
        }
        
        public class LoadPartialProductRequest : PartialProductRequest
        {
            /// <summary>
            /// Список Id статей, которые необходимо загрузить
            /// </summary>
            [Required]
            public int[] ArticleIds { get; set; }

            /// <summary>
            /// Списки Id уже загруженных статей, сгруппированные по имени контента
            /// </summary>
            public Dictionary<string, int[]> IgnoredArticleIdsByContent { get; set; } = new Dictionary<string, int[]>();
        }

        public class SavePartialProductRequest : PartialProductRequest
        {
            /// <summary>
            /// JSON части продукта, начиная с корневого контента, описанного путём <see cref="ContentPath"/>
            /// </summary>
            [Required]
            public JObject PartialProduct { get; set; }
        }

        #endregion
    }
}