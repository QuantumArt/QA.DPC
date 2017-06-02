using System;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using QA.Core;

namespace QA.DPC.Core.Helpers
{
    /// <summary>
    /// Реализует провайдер кеширования данных
    /// </summary>
    public class VersionedCacheCoreProvider : IVersionedCacheProvider
    {
        private readonly MemoryCache _cache;

        public VersionedCacheCoreProvider()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        /// <summary>
        /// Получает данные из кеша по ключу
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns></returns>
        public virtual object Get(string key)
        {
            object result = null;
            if (string.IsNullOrEmpty(key) && _cache.TryGetValue(key, out result))
            {
                return null;
            }

            return result;
        }

        /// <summary>
        /// Записывает данные в кеш
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="data">Данные</param>
        /// <param name="cacheTime">Время кеширования в секундах</param>
        public virtual void Set(string key, object data, int cacheTime)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            var policy = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now + TimeSpan.FromSeconds(cacheTime)
            };

            _cache.Set(key, data, policy);
        }

        /// <summary>
        /// Записывает данные в кеш
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="data">Данные</param>
        /// <param name="expiration">Время кеширования (sliding expiration)</param>
        public virtual void Set(string key, object data, TimeSpan expiration)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            var policy = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now + expiration
            };

            _cache.Set(key, data, policy);
        }

        /// <summary>
        /// Проверяет наличие данных в кеше
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns></returns>
        public virtual bool IsSet(string key)
        {
            object result;
            return !string.IsNullOrEmpty(key) && _cache.TryGetValue(key, out result);
        }

        public virtual bool TryGetValue(string key, out object result)
        {
            return _cache.TryGetValue(key, out result);
        }

        /// <summary>
        /// Очищает кеш
        /// </summary>
        /// <param name="key">Ключ</param>
        public virtual void Invalidate(string key)
        {

            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            _cache.Remove(key);

        }

        /// <summary>
        /// Освобождаем ресурсы
        /// </summary>
        public virtual void Dispose()
        {
        }



        #region IVersionedCacheProvider Members

        public void Add(object data, string key, string[] tags, TimeSpan expiration)
        {
            var policy = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now + expiration
            };

            if (tags != null && tags.Length > 0)
            {
                var now = DateTime.Now;
                var tagExpiration = now.AddDays(10);

                foreach (var item in tags)
                {
                    var src = AddTag(tagExpiration, item);
                    policy.AddExpirationToken(new CancellationChangeToken(src.Token));
                }
            }

            _cache.Set(key, data, policy);
        }



        public object Get(string key, string[] tags)
        {
            return Get(key);
        }

        public void InvalidateByTag(InvalidationMode mode, string tag)
        {
            Throws.IfArgumentNullOrEmpty(tag, _ => tag);
            _cache.Remove(tag);
        }

        public void InvalidateByTags(InvalidationMode mode, params string[] tags)
        {
            foreach (var tag in tags)
            {
                _cache.Remove(tag);
            }
        }

        public void Invalidate(string key, string[] tags)
        {
            Invalidate(key);
        }

        #endregion

        /// <summary>
        /// Автовосстановление кеш-тега
        /// </summary>
        private static void EvictionTagCallback(object key, object value, EvictionReason reason, object state)
        {
            (value as CancellationTokenSource)?.Cancel();

            var strkey = key as string;
            if (strkey != null)
            {
                ((VersionedCacheCoreProvider)state).AddTag(DateTime.Now.AddDays(1), strkey);
            }
        }

        private CancellationTokenSource AddTag(DateTime tagExpiration, string item)
        {
            return _cache.GetOrCreate(item, entry =>
            {
                entry.Priority = CacheItemPriority.NeverRemove;
                entry.AbsoluteExpiration = tagExpiration;
                entry.RegisterPostEvictionCallback(callback: EvictionTagCallback, state: this);
                return new CancellationTokenSource();
            });
        }
    }
}
