﻿using System;
using System.Collections.Generic;
using System.Linq;
using QA.Core.Cache;
using QA.Core.DPC.QP.Cache;
using QA.ProductCatalog.ContentProviders;
using Quantumart.QP8.BLL;

namespace QA.Core.DPC.Loader.Services
{
    public class QpCachedContextStorage : IContextStorage
    {
        private readonly VersionedCacheProviderBase _cacheProvider;

        public QpCachedContextStorage(VersionedCacheProviderBase cacheProvider)
        {
            _cacheProvider = cacheProvider;

            CacheTime = TimeSpan.FromHours(1);
        }


        private static string GetCacheKey(string qpKey)
        {
            return string.Format("Qp structure cache {0}", qpKey);
        }

        private static readonly IReadOnlyDictionary<string, string[]> StorageKeyTags = new Dictionary<string, string[]>
        {
            {"_StatusType", new[] {CacheTags.QP8.StatusType}},
            {"_User", new[] {CacheTags.QP8.User}},
            {"_FieldCache", new[] {CacheTags.QP8.Field}},
            {"_ContentCache", new[] {CacheTags.QP8.Content}},
            {"_SiteCache", new[] { CacheTags.QP8.Site}},
            {"_ContentFieldCache", new[] { CacheTags.QP8.Content, CacheTags.QP8.Field}}
        };

        public static readonly string[] TagTables = StorageKeyTags.Values.SelectMany(x => x).Distinct().ToArray();

        public T GetValue<T>(string key)
        {
            var cacheValue = _cacheProvider.Get(GetCacheKey(key));

            return cacheValue == null ? default(T) : (T)cacheValue;
        }

        public TimeSpan CacheTime { get; set; }

        public void SetValue<T>(T value, string key)
        {
            _cacheProvider.Add(value, GetCacheKey(key), StorageKeyTags[key], CacheTime);
        }

        public void ResetValue(string key)
        {
            _cacheProvider.Invalidate(GetCacheKey(key));
        }

        public IEnumerable<string> Keys
        {
            get
            {
                return StorageKeyTags.Keys;
            }
        }
    }
}
