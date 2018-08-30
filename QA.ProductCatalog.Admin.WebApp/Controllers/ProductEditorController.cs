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
using QA.Core.DPC.API.Update;

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
        
        /// <summary>
        /// CustomAction для редактирования существующего продукта
        /// </summary>
        /// <param name="content_item_id">Id корневой статьи</param>
        [HttpGet, RequireCustomAction]
        public ViewResult Edit(int content_item_id, bool isLive = false)
        {
            EditorDefinition definition = GetEditorDefinitionByArticleId(content_item_id, isLive);

            if (definition == null)
            {
                throw new InvalidOperationException($"ProductDefinition for article {content_item_id} was not found");
            }

            string editorViewPath = String.IsNullOrWhiteSpace(definition.EditorViewPath)
                ? "DefaultEditor"
                : definition.EditorViewPath;
            
            return View(editorViewPath, new ProductEditorSettingsModel
            {
                ArticleId = content_item_id,
                ProductDefinitionId = definition.ProductDefinitionId,
            });
        }

        private EditorDefinition GetEditorDefinitionByArticleId(int articleId, bool isLive)
        {
            _articleService.IsLive = isLive;

            var qpArticle = _articleService.Read(articleId);

            string productTypeField = qpArticle.FieldValues
                .Where(x => x.Field.IsClassifier)
                .Select(x => x.Value)
                .FirstOrDefault();

            int productTypeId = Int32.Parse(productTypeField);

            return _contentDefinitionService
                .GetEditorDefinition(productTypeId, qpArticle.ContentId, isLive);
        }

        /// <summary>
        /// Построить TypeScript-описание продукта.
        /// </summary>
        /// <param name="content_item_id">Id описания продукта</param>
        [HttpGet, RequireCustomAction]
        public ViewResult TypeScriptSchema(int content_item_id, bool isLive = false)
        {
            Content rootContent = _contentDefinitionService.GetDefinitionById(content_item_id, isLive);

            string jsonSchema = _jsonProductService.GetEditorJsonSchemaString(rootContent);

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
            Content rootContent = _contentDefinitionService.GetDefinitionById(content_item_id, isLive);

            ProductSchema productSchema = _editorSchemaService.GetProductSchema(rootContent);

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
        /// Получить JSON схему для редактора продукта.
        /// </summary>
        /// <param name="productDefinitionId">Id описания продукта</param>
        /// <returns>JSON схемы редактора продукта</returns>
        [HttpGet]
        public ContentResult GetEditorSchema(int productDefinitionId, bool isLive = false)
        {
            Content rootContent = _contentDefinitionService.GetDefinitionById(productDefinitionId, isLive);

            ProductSchema productSchema = _editorSchemaService.GetProductSchema(rootContent);

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

        /// <summary>
        /// Получить JSON продукта по его <see cref="Article.Id"/>.
        /// </summary>
        /// <param name="productDefinitionId">Id описания продукта</param>
        /// <param name="articleId">Id корневой статьи</param>
        /// <returns>JSON продукта</returns>
        [HttpGet]
        public ContentResult GetEditorData(int productDefinitionId, int articleId, bool isLive = false)
        {
            Content rootContent = _contentDefinitionService.GetDefinitionById(productDefinitionId, isLive);

            ArticleObject articleObject = LoadProductGraph(rootContent, articleId, isLive);

            string dataJson = JsonConvert.SerializeObject(articleObject);

            return Content(dataJson, "application/json");
        } 

        /// <summary>
        /// Загрузить часть продукта начиная с корневого контента,
        /// описанного путём <see cref="PartialProductRequest.ContentPath"/>
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
                .FindContentByPath(rootContent, request.ContentPath);

            Article[] articles = _productService
                .GetProductsByIds(partialContent, request.ArticleIds, isLive);
            
            IArticleFilter filter = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;

            ArticleObject[] articleObjects = articles
                .Select(article => _editorDataService.ConvertArticle(article, filter))
                .ToArray();

            string productsJson = JsonConvert.SerializeObject(articleObjects);

            return Content(productsJson, "application/json");
        }

        /// <summary>
        /// Загрузить связь продукта <see cref="LoadProductRelationRequest.RelationFieldName"/>
        /// начиная с корневого контента, описанного путём <see cref="PartialProductRequest.ContentPath"/>
        /// </summary>
        /// <returns>JSON связи продукта</returns>
        [HttpPost]
        public ActionResult LoadProductRelation(
             [ModelBinder(typeof(JsonModelBinder))] LoadProductRelationRequest request, bool isLive = false)
        {
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Content rootContent = _contentDefinitionService
                .GetDefinitionById(request.ProductDefinitionId, isLive);

            Content relationContent = _editorPartialContentService
                .FindContentByPath(rootContent, request.ContentPath)
                .ShallowCopy();
            
            Field relationField = relationContent.Fields
                .Single(f => f.FieldName == request.RelationFieldName);

            relationContent.Fields.Clear();
            relationContent.Fields.Add(relationField);

            ArticleObject parentObject = LoadProductGraph(relationContent, request.ParentArticleId, isLive);

            object relationObject = parentObject?[request.RelationFieldName];

            string relationJson = JsonConvert.SerializeObject(relationObject);

            return Content(relationJson, "application/json");
        }

        /// <summary>
        /// Сохранить часть продукта начиная с корневого контента,
        /// описанного путём <see cref="PartialProductRequest.ContentPath"/>.
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
                .FindContentByPath(rootContent, request.ContentPath);

            Article partialProduct = _editorDataService
                .DeserializeProduct(request.PartialProduct, partialContent);

            var partialDefinition = new ProductDefinition { StorageSchema = partialContent };

            try
            {
                // TODO: what about validation ?
                InsertData[] insertData = _productUpdateService.Update(partialProduct, partialDefinition, isLive);

                var idMappings = insertData.Select(data => new
                {
                    ClientId = data.OriginalArticleId,
                    ServerId = data.CreatedArticleId,
                });

                int partialProductId = partialProduct.Id;
                if (partialProductId <= 0)
                {
                    partialProductId = insertData
                        .Single(data => data.OriginalArticleId == partialProduct.Id)
                        .CreatedArticleId;
                }

                ArticleObject articleObject = LoadProductGraph(partialContent, partialProductId, isLive);

                string responseJson = JsonConvert.SerializeObject(new
                {
                    IdMappings = idMappings,
                    PartialProduct = articleObject,
                });

                return Content(responseJson, "application/json");
            }
            catch (ProductUpdateConcurrencyException)
            {
                ArticleObject articleObject = LoadProductGraph(partialContent, partialProduct.Id, isLive);

                string productJson = JsonConvert.SerializeObject(articleObject);

                Response.ContentType = "application/json";
                Response.Write(productJson);

                return new HttpStatusCodeResult(HttpStatusCode.Conflict);
            }
        }
        
        private ArticleObject LoadProductGraph(Content content, int articleId, bool isLive)
        {
            var productDefinition = new ProductDefinition { StorageSchema = content };

            Article article = _productService.GetProductById(articleId, isLive, productDefinition);

            if (article == null)
            {
                return null;
            }

            IArticleFilter filter = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;

            ArticleObject articleObject = _editorDataService.ConvertArticle(article, filter);

            return articleObject;
        }

#if DEBUG
        [HttpGet]
        public ViewResult ComponentLibrary()
        {
            return View();
        }

        private static readonly ConcurrentDictionary<string, ContentResult> _cache
            = new ConcurrentDictionary<string, ContentResult>();

        [HttpGet]
        public ContentResult GetEditorSchema_Test(int productDefinitionId, bool refresh = false)
        {
            string cacheKey = $"schema/{productDefinitionId}";
            if (refresh)
            {
                _cache.TryRemove(cacheKey, out var _);
            }
            return _cache.GetOrAdd(cacheKey, _ => GetEditorSchema(productDefinitionId));
        }

        [HttpGet]
        public ContentResult GetEditorData_Test(int productDefinitionId, int articleId, bool refresh = false)
        {
            string cacheKey = $"data/{productDefinitionId}/{articleId}";
            if (refresh)
            {
                _cache.TryRemove(cacheKey, out var _);
            }
            return _cache.GetOrAdd(cacheKey, _ => GetEditorData(productDefinitionId, articleId));
        }
#endif

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
        }
        
        public class LoadPartialProductRequest : PartialProductRequest
        {
            /// <summary>
            /// Список Id статей, которые необходимо загрузить
            /// </summary>
            [Required]
            public int[] ArticleIds { get; set; }
        }

        public class LoadProductRelationRequest : PartialProductRequest
        {
            /// <summary>
            /// Имя поля связи, которое необходимо загрузить
            /// </summary>
            [Required]
            public string RelationFieldName { get; set; }

            /// <summary>
            /// Id родительской статьи
            /// </summary>
            public int ParentArticleId { get; set; }
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