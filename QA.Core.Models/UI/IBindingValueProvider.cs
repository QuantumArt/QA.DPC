namespace QA.Core.Models.UI
{
    public interface IBindingValueProvider
    {
        object GetValue(DependencyProperty prop, BindingExression be, IDataContextProvider source);
    }
}
