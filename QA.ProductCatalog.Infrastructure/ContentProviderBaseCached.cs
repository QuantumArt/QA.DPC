using System;
using QA.Core;
using QA.Core.DPC.QP.Services;
using QA.Core.Cache;

namespace QA.ProductCatalog.Infrastructure
{
    public abstract class ContentProviderBaseCached<TModel> : ContentProviderBase<TModel>
        where TModel : class
    {
        private readonly TimeSpan _cacheTimeSpan = TimeSpan.FromMinutes(5);	
        private readonly IVersionedCacheProvider _cacheProvider;

        protected ContentProviderBaseCached(ISettingsService settingsService, IConnectionProvider connectionProvider, IVersionedCacheProvider cacheProvider) : base(settingsService, connectionProvider)
        {
            _cacheProvider = cacheProvider;
        }

        protected abstract string CacheKey { get; }

        public override TModel[] GetArticles()
        {
            return _cacheProvider.GetOrAdd("ContentProviderBase_" + CacheKey, GetTags(), _cacheTimeSpan, base.GetArticles);
        }
    }
}
