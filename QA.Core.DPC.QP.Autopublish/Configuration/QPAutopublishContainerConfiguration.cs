using Microsoft.Practices.Unity;
using QA.Core.DPC.QP.Autopublish.Services;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.QP.Autopublish.Configuration
{
    public class QPAutopublishContainerConfiguration : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Container.RegisterType<IAutopublishProvider, AutopublishProvider>();
            Container.RegisterType<ITask, AutopublishService>();
        }
    }
}
