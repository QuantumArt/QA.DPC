using Microsoft.Practices.Unity;
using QA.Core.DPC.QP.Servives;

namespace QA.Core.DPC.QP.Configuration
{
    public class QPConfigurationExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Container.RegisterType<IConnectionProvider, ConnectionProvider>();
            Container.RegisterType<ICustomerProvider, CustomerProvider>();
            Container.RegisterType<IIdentityProvider, IdentityProvider>();
        }
    }
}
