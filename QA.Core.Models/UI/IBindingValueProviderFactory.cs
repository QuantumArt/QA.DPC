using System;
namespace QA.Core.Models.UI
{
    public interface IBindingValueProviderFactory
    {
        IBindingValueProvider GetBindingValueProvider(DependencyObject obj);
    }
}
