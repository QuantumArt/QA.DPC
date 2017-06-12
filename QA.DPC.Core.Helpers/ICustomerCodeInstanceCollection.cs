using System;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.Infrastructure;

namespace QA.DPC.Core.Helpers
{
    public interface ICustomerCodeInstanceCollection
    {
        CustomerCodeInstance Get(IIdentityProvider provider, IConnectionProvider connectionProvider);

        CustomerCodeTaskInstance Get(IIdentityProvider provider, Func<ITask> reindexAllTaskAccessor);

        
    }
}