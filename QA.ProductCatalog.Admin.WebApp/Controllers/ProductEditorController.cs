using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QA.Core.DPC.Loader.Editor;
using QA.Core.DPC.Loader.Services;
using QA.Core.Models;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Admin.WebApp.Models;
using QA.ProductCatalog.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using QA.ProductCatalog.Admin.WebApp.Filters;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    [RequireCustomAction]
    public class ProductEditorController : Controller
    {
        protected readonly IContentDefinitionService _contentDefinitionService;
        protected readonly IProductService _productService;
        protected readonly IReadOnlyArticleService _articleService;
        protected readonly EditorSchemaService _editorSchemaService;
        protected readonly EditorDataService _editorDataService;
        protected readonly EditorPartialContentService _editorPartialContentService;
        private readonly EditorLocaleService _editorLocaleService;

        public ProductEditorController(
            IContentDefinitionService contentDefinitionService,
            IProductService productService,
            IReadOnlyArticleService articleService,
            EditorSchemaService editorSchemaService,
            EditorDataService editorDataService,
            EditorPartialContentService editorPartialContentService,
            EditorLocaleService editorLocaleService)
        {
            _contentDefinitionService = contentDefinitionService;
            _productService = productService;
            _articleService = articleService;
            _editorSchemaService = editorSchemaService;
            _editorDataService = editorDataService;
            _editorPartialContentService = editorPartialContentService;
            _editorLocaleService = editorLocaleService;
        }
        
        [HttpGet]
        public ActionResult Edit(int content_item_id, bool isLive = false)
        {
            EditorDefinition definition = GetEditorDefinitionByArticleId(content_item_id, isLive);

            if (definition == null)
            {
                throw new InvalidOperationException($"ProductDefinition for article {content_item_id} was not found");
            }

            string editorViewPath = String.IsNullOrWhiteSpace(definition.EditorViewPath)
                ? "DefaultEditor"
                : definition.EditorViewPath;

            CultureInfo currentUserCulture = _editorLocaleService.GetCurrentUserCulture();
            
            return View(editorViewPath, new ProductEditorSettingsModel
            {
                ArticleId = content_item_id,
                ProductDefinitionId = definition.ProductDefinitionId,
                UserLocale = currentUserCulture?.Name,
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

            Int32.TryParse(productTypeField, out int productTypeId);

            return _contentDefinitionService
                .GetEditorDefinition(productTypeId, qpArticle.ContentId, isLive);
        }
        
        [HttpGet]
        public ActionResult TypeScriptSchema(int content_item_id, bool isLive = false)
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
        
        protected ArticleObject LoadProductGraph(Content content, int articleId, bool isLive)
        {
            var productDefinition = new ProductDefinition { StorageSchema = content };

            Article article = _productService.GetProductById(articleId, isLive, productDefinition);

            if (article == null)
            {
                return null;
            }

            IArticleFilter filter = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;

            ArticleObject articleObject = _editorDataService.ConvertArticle(article, filter, new HashSet<Article>());

            return articleObject;
        }

#if DEBUG
        [HttpGet("ComponentLibrary")]
        public ActionResult ComponentLibrary()
        {
            return View();
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
        
        #endregion
    }
}