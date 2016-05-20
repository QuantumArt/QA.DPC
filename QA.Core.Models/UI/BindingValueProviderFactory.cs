using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QA.Core.Models.UI
{
    public class BindingValueProviderFactory : IBindingValueProviderFactory
    {
        static Lazy<IBindingValueProviderFactory> _default = new Lazy<IBindingValueProviderFactory>(()
            => new DefaultBindingValueProviderFactory(), LazyThreadSafetyMode.ExecutionAndPublication);
        
        private static IBindingValueProviderFactory _current;

        public static IBindingValueProviderFactory Current
        {
            get
            {
                if (_current == null)
                    return _default.Value;
                else
                {
                    return _current;
                }
            }
            set { _current = value; }
        }

        public IBindingValueProvider GetBindingValueProvider(DependencyObject obj)
        {
            return Current.GetBindingValueProvider(obj);
        }
    }
}
