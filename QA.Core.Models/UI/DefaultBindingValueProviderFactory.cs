using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.Models.UI
{
    public class DefaultBindingValueProviderFactory : IBindingValueProviderFactory
    {
        private IBindingValueProvider _bvp;
        public DefaultBindingValueProviderFactory()
            : this(new ReflectedBindingValueProvider()) { }

        public DefaultBindingValueProviderFactory(IBindingValueProvider bvp)
        {
            _bvp = bvp;
        }

        public IBindingValueProvider GetBindingValueProvider(DependencyObject obj)
        {
            return _bvp;
        }
    }
}
