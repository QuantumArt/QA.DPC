using System;
namespace QA.Core.Models.UI
{
    public interface IDataContextProviderSetter
    {
        void ApplyProvider(IDataContextProvider provider);
    }
}
