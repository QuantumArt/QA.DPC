using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using Unity;
using Unity.Extension;
using Unity.Injection;

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
