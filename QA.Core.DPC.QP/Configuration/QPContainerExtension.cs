using Microsoft.Practices.Unity;
using QA.Core.DPC.QP.Servives;

namespace QA.Core.DPC.QP.Configuration
{
    public static class QPContainerExtension
    {
        public static string GetCustomerCode(this IUnityContainer container)
        {
            return container.Resolve<IIdentityProvider>().Identity.CustomerCode;
        }
    }
}
