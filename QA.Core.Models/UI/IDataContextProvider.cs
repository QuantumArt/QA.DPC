using System;
namespace QA.Core.Models.UI
{
    public interface IDataContextProvider
    {
        //[Obsolete]
        object GetDataContext(int skip);
        object GetDataContext(BindingExression expr);
        [Obsolete]
        object GetRootContext(int skip);
    }
}
