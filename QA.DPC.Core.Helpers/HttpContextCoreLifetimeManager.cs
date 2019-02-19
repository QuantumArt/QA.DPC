using System;
using System.Threading;
using Unity.Lifetime;
using Microsoft.AspNetCore.Http;

namespace QA.Core.Web
{
    /// <summary>
    /// Реализует жизненный цикл объекта в текущем запросе.
    /// Если контекст запроса пуст, то используется стратегия ThreadLocal&lt;T&gt;.
    /// </summary>
    public class HttpContextCoreLifetimeManager : LifetimeManager, IDisposable, ITypeLifetimeManager
    {

        private readonly IHttpContextAccessor _httpContextAccessor; 
        
        public HttpContextCoreLifetimeManager(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        private ThreadLocal<object> _val = new ThreadLocal<object>();

        /// <summary>
        /// Ключ для хранения в контексте запроса
        /// </summary>
        protected string Key { get; }

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

        /// <summary>
        /// Создание LifetimeManagera
        /// </summary>
        /// <returns></returns>
        protected override LifetimeManager OnCreateLifetimeManager()
        {
            throw new NotImplementedException();
        }
    }
}
