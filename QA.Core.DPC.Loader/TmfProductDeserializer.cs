using Microsoft.AspNetCore.Http;
using QA.Core.Cache;
using QA.Core.DPC.QP.Services;
using QA.Core.Models.Entities;
using QA.Core.ProductCatalog.Actions.Services;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository.ArticleMatching;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Conditions;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Models;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QPublishing.Database;
using System;
using System.IO;
using System.Linq;
using Article = QA.Core.Models.Entities.Article;

namespace QA.Core.DPC.Loader
{
    internal class TmfProductDeserializer : ProductDeserializer
    {
        public const string TmfIdFieldName = "TmfId";

        protected readonly IArticleMatchService<ConditionBase> ArticleMatchService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private int _nonExistentArticleId = -1;

        public TmfProductDeserializer(
            IFieldService fieldService,
            IServiceFactory serviceFactory,
            ICacheItemWatcher cacheItemWatcher,
            IContextStorage contextStorage,
            IConnectionProvider connectionProvider,
            IArticleMatchService<ConditionBase> articleMatchService,
            IHttpContextAccessor httpContextAccessor)
            : base(fieldService, serviceFactory, cacheItemWatcher, contextStorage, connectionProvider)
        {
            ArticleMatchService = articleMatchService;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override void ApplyArticleField(Article article, ArticleField field, IProductDataSource productDataSource)
        {
            base.ApplyArticleField(article, field, productDataSource);
            TryResolveTmfId(productDataSource, article, field);
        }

        protected override Article DeserializeArticle(
            IProductDataSource productDataSource,
            Models.Configuration.Content definition,
            DBConnector connector,
            Context context)
        {
            var article = base.DeserializeArticle(productDataSource, definition, connector, context);

            if (HttpMethods.IsPost(_httpContextAccessor.HttpContext.Request.Method)
                && article is not null)
            {
                // TODO: Remove after implementing support for default values in QP.
                if (TryGetPlainField(article, "PriceType", out var priceType))
                {
                    priceType.NativeValue = priceType.Value = "recurring";
                }

                if (TryGetPlainField(article, "LifecycleStatus", out var lifecycleStatus))
                {
                    lifecycleStatus.NativeValue = lifecycleStatus.Value = "Active";
                }
            }

            return article;
        }

        private static bool TryGetPlainField(Article article, string name, out PlainArticleField foundField)
        {
            if (article.Fields.TryGetValue(name, out var field)
                && field is PlainArticleField plainField)
            {
                foundField = plainField;
                return true;
            }

            foundField = null;
            return false;
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
                return _nonExistentArticleId--;
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

            string tmfArticleId = productDataSource.GetTmfArticleId();
            if (string.IsNullOrWhiteSpace(tmfArticleId))
            {
                plainField.NativeValue = plainField.Value = Guid.NewGuid().ToString();
                return;
            }

            plainField.NativeValue = plainField.Value = tmfArticleId;

            article.Id = ResolveArticleId(tmfArticleId, articleField.ContentId ?? default);
        }

        private int ResolveArticleId(string externalId, int contentId)
        {
            _ = _contentService.Read(contentId);

            var queryField = new QueryField { ContentId = contentId, Name = TmfIdFieldName };
            var condition = new ComparitionCondition(
                new[] { queryField },
                externalId,
                "=");

            var productArticle = ArticleMatchService
                .MatchArticles(contentId, condition, MatchMode.Strict)
                .SingleOrDefault();

            return productArticle?.Id ?? _nonExistentArticleId--;
        }
    }
}
