using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.DPC.QP.Servives
{
    public class CustomerCacheProvider : IVersionedCacheProvider
    {
        private readonly IVersionedCacheProvider _cacheProvider;
        private readonly IIdentityProvider _identityProvider;
        public CustomerCacheProvider(IVersionedCacheProvider cacheProvider, IIdentityProvider identityProvider)
        {
            _cacheProvider = cacheProvider;
            _identityProvider = identityProvider;
        }

        public void Add(object data, string key, string[] tags, TimeSpan expiration)
        {
            _cacheProvider.Add(data, GetKey(key), tags, expiration);
        }

        public void Dispose()
        {
            _cacheProvider.Dispose();
        }

        public object Get(string key)
        {
            return _cacheProvider.Get(GetKey(key));
        }

        public object Get(string key, string[] tags)
        {
            return _cacheProvider.Get(GetKey(key), tags);
        }

        public void Invalidate(string key)
        {
            _cacheProvider.Invalidate(GetKey(key));
        }

        public void Invalidate(string key, string[] tags)
        {
            _cacheProvider.Invalidate(GetKey(key), tags);
        }

        public void InvalidateByTag(InvalidationMode mode, string tag)
        {
            _cacheProvider.InvalidateByTag(mode, tag);
        }

        public void InvalidateByTags(InvalidationMode mode, params string[] tags)
        {
            _cacheProvider.InvalidateByTags(mode, tags);
        }

        public bool IsSet(string key)
        {
            return _cacheProvider.IsSet(GetKey(key));
        }

        public void Set(string key, object data, TimeSpan expiration)
        {
            _cacheProvider.Set(GetKey(key), data, expiration);
        }

        public void Set(string key, object data, int cacheTime)
        {
            _cacheProvider.Set(GetKey(key), data, cacheTime);
        }

        public bool TryGetValue(string key, out object result)
        {
            return _cacheProvider.TryGetValue(GetKey(key), out result);
        }

        private string GetKey(string baseKey)
        {
            if (_identityProvider.Identity == null)
            {
                return baseKey;
            }
            else
            {
                return $"{_identityProvider.Identity.CustomerCode}_{baseKey}";
            }            
        }
    }
}
