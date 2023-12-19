using System;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.ContentProviders;

namespace QA.DPC.Core.Helpers
{
    public interface ICustomerCodeTaskInstanceCollection
    {
        CustomerCodeTaskInstance Get(IIdentityProvider provider, ITask reindexAllTask);
    }
}