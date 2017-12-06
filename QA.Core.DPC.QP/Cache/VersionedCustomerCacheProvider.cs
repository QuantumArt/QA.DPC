using System;
using System.Runtime.Caching;

namespace QA.Core.DPC.QP.Cache
{
    /// <summary>
    /// Реализует провайдер кеширования данных
    /// </summary>
    public class VersionedCustomerCacheProvider : IVersionedCacheProvider
    {
        private readonly MemoryCache _cache;

        public VersionedCustomerCacheProvider(string customerCode)
        {
            if (customerCode == null)
            {
                _cache = MemoryCache.Default;
            }
            else
            {
                _cache = new MemoryCache(customerCode);
            }
        }

        /// <summary>
        /// Получает данные из кеша по ключу
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns></returns>
        public virtual object Get(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            return _cache[key];
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

            var policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTime.Now + TimeSpan.FromSeconds(cacheTime);
            _cache.Set(new CacheItem(key, data), policy);
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

            var policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTime.Now + expiration;
            _cache.Set(new CacheItem(key, data), policy);
        }

        /// <summary>
        /// Проверяет наличие данных в кеше
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns></returns>
        public virtual bool IsSet(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            return _cache.Contains(key);
        }

        public virtual bool TryGetValue(string key, out object result)
        {
            result = _cache[key];
            return result != null;
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
            _cache.Dispose();
        }



        #region IVersionedCacheProvider Members

        public void Add(object data, string key, string[] tags, TimeSpan expiration)
        {
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.Now + expiration
            };

            if (tags != null && tags.Length > 0)
            {
                var now = DateTime.Now;
                var tagExpiration = now.AddDays(10);

                foreach (var item in tags)
                {
                    AddTag(_cache, now, tagExpiration, item);
                }

                policy.ChangeMonitors.Add(
                   _cache.CreateCacheEntryChangeMonitor(tags)
                );
            }

            _cache.Set(key, data, policy);
        }



        public object Get(string key, string[] tags)
        {
            return this.Get(key);
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
            this.Invalidate(key);
        }

        #endregion

        /// <summary>
        /// Автовосстановление кеш-тега
        /// </summary>
        /// <param name="arguments"></param>
        private static void CacheTagRemovedCallBack(CacheEntryRemovedArguments arguments)
        {
            var now = DateTime.Now;
            var tagExpiration = now.AddDays(1);

            if (arguments.CacheItem != null)
            {
                AddTag(arguments.Source, now, tagExpiration, arguments.CacheItem.Key);
            }
        }

        private static void AddTag(ObjectCache cache, DateTime now, DateTime tagExpiration, string item)
        {
            cache.AddOrGetExisting(item, now, new CacheItemPolicy
            {
                Priority = CacheItemPriority.NotRemovable,
                AbsoluteExpiration = tagExpiration,
                RemovedCallback = CacheTagRemovedCallBack
            });
        }
    }
}
