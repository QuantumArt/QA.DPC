using System;
using Microsoft.Extensions.Caching.Memory;
using QA.Core.Cache;
using QA.Core.DPC.QP.Services;
using QA.DotNetCore.Caching.Interfaces;
using QA.DotNetCore.Engine.Persistent.Interfaces;

namespace QA.ProductCatalog.ContentProviders
{
    public abstract class ContentProviderBaseCached<TModel> : ContentProviderBase<TModel>
        where TModel : class
    {
        private readonly TimeSpan _cacheTimeSpan = TimeSpan.FromMinutes(5);	
        private readonly VersionedCacheProviderBase _cacheProvider;

        protected ContentProviderBaseCached(
            ISettingsService settingsService, 
            IConnectionProvider connectionProvider, 
            VersionedCacheProviderBase cacheProvider,
            IQpContentCacheTagNamingProvider namingProvider,
            IUnitOfWork unitOfWork) : base(settingsService, connectionProvider, namingProvider, unitOfWork)
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
