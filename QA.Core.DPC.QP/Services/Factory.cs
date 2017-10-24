using System;
using System.Collections.Concurrent;

namespace QA.Core.DPC.QP.Services
{
    public abstract class Factory<T> : IFactory<T>
        where T : class
    {
        private readonly ConcurrentDictionary<string, T> _dictionary;
        private readonly IIdentityProvider _identityProvider;

        public Factory(IIdentityProvider identityProvider)
        {
            _identityProvider = identityProvider;
            _dictionary = new ConcurrentDictionary<string, T>();
        }

        public T Create()
        {
            var key = _identityProvider.Identity.CustomerCode;
            var value = _dictionary.GetOrAdd(key, CreateInstance);
            return value;
        }

        protected abstract T CreateInstance(string customerCode);
    }

    public class CustomFactory<T> : Factory<T>
        where T : class
    {
        private readonly Func<string, T> _factory;
        public CustomFactory(IIdentityProvider identityProvider, Func<string, T> factory)
            : base(identityProvider)
        {
            _factory = factory;
        }

        protected override T CreateInstance(string customerCode)
        {
            return _factory(customerCode);
        }
    }
}
