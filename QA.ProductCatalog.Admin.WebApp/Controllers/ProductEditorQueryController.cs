using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QA.Core.DPC.Loader.Editor;
using QA.Core.DPC.Loader.Services;
using QA.Core.Models;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Admin.WebApp.Binders;
using QA.ProductCatalog.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.SessionState;
using System.ComponentModel.DataAnnotations;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    [RoutePrefix("ProductEditorQuery")]
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class ProductEditorQueryController : ProductEditorController
    {
        private readonly PublicationStatusService _publicationStatusService;
        private readonly EditorCustomActionService _editorCustomActionService;
        private readonly EditorPreloadingService _editorPreloadingService;

        public ProductEditorQueryController(
            // base dependencies
            IContentDefinitionService contentDefinitionService,
            IProductService productService,
            IReadOnlyArticleService articleService,
            EditorSchemaService editorSchemaService,
            EditorDataService editorDataService,
            EditorPartialContentService editorPartialContentService,
            // self dependencies
            PublicationStatusService publicationStatusService,
            EditorPreloadingService editorPreloadingService,
            EditorCustomActionService editorCustomActionService)
            : base(contentDefinitionService,
                productService,
                articleService,
                editorSchemaService,
                editorDataService,
                editorPartialContentService)
        {
            _publicationStatusService = publicationStatusService;
            _editorPreloadingService = editorPreloadingService;
            _editorCustomActionService = editorCustomActionService;
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
        /// Получить максимальное время обновления продукта глобально по всей витрине
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetMaxPublicationTime()
        {
            DateTime? timestamp = await _publicationStatusService.GetMaxPublicationTime();

            string json = JsonConvert.SerializeObject(timestamp);

            return Content(json, "application/json");
        }

        /// <summary>
        /// Получить последнее время обновления заданных продуктов на Live и Stage
        /// </summary>
        /// <param name="productIds"> Список Id продуктов </param>
        /// <returns>Словарь с временами обновления продуктов по Id продукта</returns>
        [HttpPost]
        public async Task<ActionResult> GetPublicationTimestamps(
            [ModelBinder(typeof(JsonModelBinder))] int[] productIds)
        {
            var timestamps = await _publicationStatusService.GetProductTimestamps(productIds);

            var timestampsById = timestamps
                .GroupBy(t => t.ProductId)
                .ToDictionary(g => g.Key, g => new
                {
                    Live = g.Where(t => t.IsLive).DefaultIfEmpty().Max(t => t?.Updated),
                    Stage = g.Where(t => !t.IsLive).DefaultIfEmpty().Max(t => t?.Updated)
                });

            string json = JsonConvert.SerializeObject(timestampsById);

            return Content(json, "application/json");
        }

        /// <summary>
        /// Получить последнее время обновления на Live и Stage для всех продуктов,
        /// обновленных после <paramref name="updatedSince"/>.
        /// </summary>
        /// <param name="updatedSince"> Время предыдущего запроса </param>
        /// <returns>Словарь с временами обновления продуктов по Id продукта</returns>
        [HttpGet, OutputCache(
            Duration = 4,
            Location = OutputCacheLocation.Server,
            VaryByParam = "customerCode;updatedSince")]
        public async Task<ActionResult> GetPublicationTimestamps(DateTime updatedSince)
        {
            var timestamps = await _publicationStatusService.GetProductTimestamps(updatedSince);

            var timestampsById = timestamps
                .GroupBy(t => t.ProductId)
                .ToDictionary(g => g.Key, g => new
                {
                    Live = g.Where(t => t.IsLive).DefaultIfEmpty().Max(t => t?.Updated),
                    Stage = g.Where(t => !t.IsLive).DefaultIfEmpty().Max(t => t?.Updated)
                });

            string json = JsonConvert.SerializeObject(timestampsById);

            return Content(json, "application/json");
        }

        /// <summary>
        /// Получить информацию о CustomAction по его Alias
        /// <param name="alias">Alias</param>
        /// <returns>ActionCode и EntityTypeCode для Action</returns>
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetCustomActionByAlias(string alias)
        {
            CustomActionInfo actionInfo = await _editorCustomActionService.GetCustomActionByAlias(alias);

            string json = JsonConvert.SerializeObject(actionInfo);

            return Content(json, "application/json");
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

            var visited = new HashSet<Article>();

            ArticleObject[] articleObjects = articles
                .Select(article => _editorDataService.ConvertArticle(article, filter, visited))
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
        /// Загрузить все возможные статьи для связи продукта <see cref="LoadProductRelationRequest.RelationFieldName"/>
        /// начиная с корневого контента, описанного путём <see cref="PartialProductRequest.ContentPath"/>
        /// </summary>
        /// <returns>JSON статей связи продукта</returns>
        [HttpPost]
        public ActionResult PreloadRelationArticles(
             [ModelBinder(typeof(JsonModelBinder))] PreloadRelationArticlesRequest request, bool isLive = false)
        {
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Content rootContent = _contentDefinitionService
                .GetDefinitionById(request.ProductDefinitionId, isLive);

            Content relationContent = _editorPartialContentService
                .FindContentByPath(rootContent, request.ContentPath);

            EntityField relationField = (EntityField)relationContent.Fields
                .Single(f => f.FieldName == request.RelationFieldName);

            ArticleObject[] preloadedArticles = _editorPreloadingService
                .PreloadRelationArticles(relationField, new HashSet<Article>());

            string relationJson = JsonConvert.SerializeObject(preloadedArticles);

            return Content(relationJson, "application/json");
        }

#if DEBUG
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

        public class PreloadRelationArticlesRequest : PartialProductRequest
        {
            /// <summary>
            /// Имя поля связи, которое необходимо загрузить
            /// </summary>
            [Required]
            public string RelationFieldName { get; set; }
        }

        #endregion
    }
}