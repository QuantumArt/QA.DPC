using QA.Core.Models.UI;

namespace QA.Core.DPC.UI
{
    public class QPModelBindingFactory : IBindingValueProviderFactory
    {
        private QPModelBindingValueProvider _vp;
        public QPModelBindingFactory()
        {
            _vp = new QPModelBindingValueProvider();
        }

        public IBindingValueProvider GetBindingValueProvider(DependencyObject obj)
        {
            return _vp;
        }
    }
}
