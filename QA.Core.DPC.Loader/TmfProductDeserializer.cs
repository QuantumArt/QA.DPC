using QA.Core.Cache;
using QA.Core.DPC.QP.Services;
using QA.Core.Models.Entities;
using QA.Core.ProductCatalog.Actions.Services;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository.ArticleMatching;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Conditions;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Models;
using Quantumart.QP8.BLL.Services.API;
using System;
using System.Linq;
using Article = QA.Core.Models.Entities.Article;

namespace QA.Core.DPC.Loader
{
    internal class TmfProductDeserializer : ProductDeserializer
    {
        public const string TmfIdFieldName = "TmfId";

        protected readonly IArticleMatchService<ConditionBase> _articleMatchService;

        public TmfProductDeserializer(
            IFieldService fieldService,
            IServiceFactory serviceFactory,
            ICacheItemWatcher cacheItemWatcher,
            IContextStorage contextStorage,
            IConnectionProvider connectionProvider,
            IArticleMatchService<ConditionBase> articleMatchService)
            : base(fieldService, serviceFactory, cacheItemWatcher, contextStorage, connectionProvider)
        {
            _articleMatchService = articleMatchService;
        }

        protected override void ApplyArticleField(Article article, ArticleField field, IProductDataSource productDataSource)
        {
            base.ApplyArticleField(article, field, productDataSource);
            TryResolveTmfId(productDataSource, article, field);
        }

        protected override int GetArticleId(IProductDataSource productDataSource, int contentId)
        {
            int articleId = base.GetArticleId(productDataSource, contentId);
            if (articleId > 0)
            {
                return articleId;
            }

            string tmfArticleId = productDataSource.GetTmfArticleId();
            if (string.IsNullOrWhiteSpace(tmfArticleId))
            {
                return default;
            }

            return ResolveArticleId(tmfArticleId, contentId);
        }

        private void TryResolveTmfId(IProductDataSource productDataSource, Article article, ArticleField articleField)
        {
            if (!articleField.FieldName.Equals(TmfIdFieldName, StringComparison.OrdinalIgnoreCase)
                || articleField is not PlainArticleField plainField)
            {
                return;
            }

            string tmfId = productDataSource.GetTmfArticleId();
            if (string.IsNullOrWhiteSpace(tmfId))
            {
                return;
            }

            plainField.Value = tmfId;
            plainField.NativeValue = tmfId;

            article.Id = ResolveArticleId(tmfId, articleField.ContentId ?? default);
        }

        private int ResolveArticleId(string externalId, int contentId)
        {
            _ = _contentService.Read(contentId);

            var queryField = new QueryField { ContentId = contentId, Name = TmfIdFieldName };
            var condition = new ComparitionCondition(
                new[] { queryField },
                externalId,
                "=");

            var productArticle = _articleMatchService
                .MatchArticles(contentId, condition, MatchMode.Strict)
                .SingleOrDefault();

            return productArticle?.Id ?? default;
        }
    }
}
