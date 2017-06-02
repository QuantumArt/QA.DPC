using Microsoft.Practices.Unity;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.Core.DPC.QP.Servives;

namespace QA.Core.DPC.QP.Configuration
{
    public class QPContainerConfiguration : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Container.RegisterType<IConnectionProvider, ConnectionProvider>(new InjectionConstructor(typeof(ICustomerProvider), typeof(IIdentityProvider), Service.Admin));
            Container.RegisterType<ICustomerProvider, CustomerProvider>();
            Container.RegisterType<IIdentityProvider, IdentityProvider>();
        }
    }
}
