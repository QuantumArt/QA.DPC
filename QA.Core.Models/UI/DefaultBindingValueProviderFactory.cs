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
