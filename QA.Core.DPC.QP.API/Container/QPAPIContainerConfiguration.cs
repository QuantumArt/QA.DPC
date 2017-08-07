using Microsoft.Practices.Unity;
using Newtonsoft.Json.Linq;
using QA.Core.DPC.QP.API.Services;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.QP.API.Container
{
    public class QPAPIContainerConfiguration : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Container.RegisterType<IProductSimpleAPIService, TarantoolProductAPIService>();
            Container.RegisterType<IProductSimpleService<JToken, JToken>, TarantoolJsonService>();
            Container.RegisterType<IStatusProvider, StatusProvider>();
        }
    }
}