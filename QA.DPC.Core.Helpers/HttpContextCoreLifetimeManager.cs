using System;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Unity.Lifetime;

namespace QA.DPC.Core.Helpers
{
    /// <summary>
    /// Реализует жизненный цикл объекта в текущем запросе.
    /// Если контекст запроса пуст, то используется стратегия ThreadLocal&lt;T&gt;.
    /// </summary>
    public class HttpContextCoreLifetimeManager : LifetimeManager, IDisposable, ITypeLifetimeManager, IFactoryLifetimeManager
    {
        
        private readonly string _itemName;
        private readonly IHttpContextAccessor _httpContextAccessor; 
        
        public HttpContextCoreLifetimeManager(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _itemName = Guid.NewGuid().ToString();
        }
        
        private AsyncLocal<object> _val = new AsyncLocal<object>();

        /// <summary>
        /// Ключ для хранения в контексте запроса
        /// </summary>
        protected string Key => _itemName;

        /// <summary>
        /// Возвращает значение
        /// </summary>
        /// <returns></returns>
        public override object GetValue(ILifetimeContainer container = null)
        {
            var ctx = _httpContextAccessor.HttpContext;

            if (ctx == null)
            {
                return _val == null ? null : _val.Value;
            }

            return ctx.Items[Key];
        }

        protected override LifetimeManager OnCreateLifetimeManager()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Удаление значения
        /// </summary>
        public void RemoveValue()
        {
            var disposable = GetValue() as IDisposable;
            var ctx = _httpContextAccessor.HttpContext;

            if (ctx != null)
            {
                ctx.Items.Remove(Key);
            }

            _val = null;

            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        /// <summary>
        /// Устанавливает значение
        /// </summary>
        /// <param name="newValue"></param>
        public void SetValue(object newValue)
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx == null)
            {
                _val.Value = newValue;
            }
            else
            {
                ctx.Items[Key] = newValue;
            }
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public void Dispose()
        {
            RemoveValue();
        }

    }
}
