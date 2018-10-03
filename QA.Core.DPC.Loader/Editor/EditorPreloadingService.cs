using System;
using System.Linq;
using QA.Core.DPC.Loader.Services;
using QA.Core.Models;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL.Services.API;
using QP8BLL = Quantumart.QP8.BLL;

namespace QA.Core.DPC.Loader.Editor
{
    public class EditorPreloadingService
    {
        private readonly FieldService _fieldService;
        private readonly IReadOnlyArticleService _articleService;
        private readonly IProductService _productService;
        private readonly EditorDataService _editorDataService;

        public EditorPreloadingService(
            FieldService fieldService,
            IReadOnlyArticleService articleService,
            IProductService productService,
            EditorDataService editorDataService)
        {
            _fieldService = fieldService;
            _articleService = articleService;
            _productService = productService;
            _editorDataService = editorDataService;
        }

        public string GetRelationCondition(EntityField entityField, QP8BLL.Field qpField)
        {
            if (!String.IsNullOrWhiteSpace(entityField.RelationCondition))
            {
                return entityField.RelationCondition;
            }
            if (!(entityField is BackwardRelationField)
                && qpField.UseRelationCondition
                && !String.IsNullOrWhiteSpace(qpField.RelationCondition))
            {
                return qpField.RelationCondition;
            }
            return null;
        }

        /// <exception cref="InvalidOperationException"/>
        public ArticleObject[] PreloadRelationArticles(EntityField entityField, Dictionaries dictionaries = null)
        {
            QP8BLL.Field qpField = _fieldService.Read(entityField.FieldId);

            string relationCondition = GetRelationCondition(entityField, qpField);

            return PreloadRelationArticles(entityField, relationCondition, dictionaries);
        }

        /// <exception cref="InvalidOperationException"/>
        public ArticleObject[] PreloadRelationArticles(
            EntityField entityField, string relationCondition, Dictionaries dictionaries = null)
        {
            if (entityField.PreloadingMode == PreloadingMode.None)
            {
                throw new InvalidOperationException($"Preloading for EntityField ({entityField.FieldId}) is disabled");
            }

            Content content = entityField.Content;
            if (dictionaries != null && !content.Fields.OfType<Dictionaries>().Any())
            {
                content = content.ShallowCopy();
                content.Fields.Add(dictionaries);
            }

            int[] articleIds = _articleService
                .Ids(content.ContentId, null, filter: relationCondition ?? "")
                .ToArray();

            Article[] articles = _productService.GetProductsByIds(content, articleIds);

            ArticleObject[] articleObjects = articles
                .Select(a => _editorDataService.ConvertArticle(a, ArticleFilter.DefaultFilter))
                .ToArray();

            return articleObjects;
        }
    }
}
