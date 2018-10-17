using System;
using Microsoft.Extensions.Caching.Memory;
using QA.Core.Cache;
using QA.Core.DPC.QP.Services;

namespace QA.ProductCatalog.ContentProviders
{
    public abstract class ContentProviderBaseCached<TModel> : ContentProviderBase<TModel>
        where TModel : class
    {
        private readonly TimeSpan _cacheTimeSpan = TimeSpan.FromMinutes(5);	
        private readonly IVersionedCacheProvider2 _cacheProvider;

        protected ContentProviderBaseCached(ISettingsService settingsService, IConnectionProvider connectionProvider, IVersionedCacheProvider2 cacheProvider) : base(settingsService, connectionProvider)
        {
            _cacheProvider = cacheProvider;
        }

        protected abstract string CacheKey { get; }

        public override TModel[] GetArticles()
        {
            return _cacheProvider.GetOrAdd("ContentProviderBase_" + CacheKey, GetTags(), _cacheTimeSpan, base.GetArticles, true, CacheItemPriority.NeverRemove);
        }
    }
}
