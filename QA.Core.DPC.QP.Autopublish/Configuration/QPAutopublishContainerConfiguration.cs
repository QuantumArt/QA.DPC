using QA.Core.DPC.QP.Autopublish.Services;
using QA.ProductCatalog.ContentProviders;
using Unity;
using Unity.Extension;

namespace QA.Core.DPC.QP.Autopublish.Configuration
{
    public class QPAutopublishContainerConfiguration : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Container.RegisterType<IAutopublishProcessor, AutopublishProcessor>();
            Container.RegisterType<IAutopublishProvider, AutopublishProvider>();
            Container.RegisterType<ITask, AutopublishService>();
        }
    }
}
