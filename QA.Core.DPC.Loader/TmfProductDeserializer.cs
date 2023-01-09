using Microsoft.AspNetCore.Http;
using QA.Core.Cache;
using QA.Core.DPC.QP.Services;
using QA.Core.Models.Entities;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.ContentProviders;
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
        private readonly string _tmfIdFieldName;

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
            IHttpContextAccessor httpContextAccessor,
            ISettingsService settingsService)
            : base(fieldService, serviceFactory, cacheItemWatcher, contextStorage, connectionProvider)
        {
            ArticleMatchService = articleMatchService;
            _httpContextAccessor = httpContextAccessor;
            _tmfIdFieldName = settingsService.GetSetting(SettingsTitles.TMF_ID_FIELD_NAME);
        }

        protected override void ApplyArticleField(Article article, ArticleField field, IProductDataSource productDataSource)
        {
            base.ApplyArticleField(article, field, productDataSource);

            PlainArticleField plainField = CastTmfIdArticleField(field);
            if (plainField is null)
            {
                return;
            }

            if (!TryReadTmfIdFromDataSource(productDataSource, out var tmfArticleId))
            {
                plainField.NativeValue = plainField.Value = Guid.NewGuid().ToString();
                return;
            }

            plainField.NativeValue = plainField.Value = tmfArticleId;
            article.Id = ResolveArticleId(tmfArticleId, field.ContentId ?? default);
        }

        private PlainArticleField CastTmfIdArticleField(ArticleField articleField)
        {
            return articleField.FieldName.Equals(_tmfIdFieldName, StringComparison.OrdinalIgnoreCase)
                ? articleField as PlainArticleField
                : null;
        }

        private static bool TryReadTmfIdFromDataSource(IProductDataSource productDataSource, out string tmfArticleId)
        {
            tmfArticleId = productDataSource.GetTmfArticleId();
            return !string.IsNullOrWhiteSpace(tmfArticleId);
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

        private int ResolveArticleId(string externalId, int contentId)
        {
            var queryField = new QueryField { ContentId = contentId, Name = _tmfIdFieldName };
            var condition = new ComparitionCondition(
                new[] { queryField },
                externalId,
                "=");

            var foundArticles = ArticleMatchService
                .MatchArticles(contentId, condition, MatchMode.Strict);

            return foundArticles.Length switch
            {
                0 => _nonExistentArticleId--,
                1 => foundArticles[0].Id,
                _ => throw new InvalidOperationException(
                    $"More then one article found with {_tmfIdFieldName}={externalId}: " +
                    string.Join(",", foundArticles.Select(a => $"{a.ContentId}:{a.Id}"))),
            };
        }
    }
}
