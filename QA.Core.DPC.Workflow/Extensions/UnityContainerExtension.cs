using Microsoft.Extensions.Options;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration.DAL;
using QA.Workflow.Models;
using Unity;

namespace QA.Core.DPC.Workflow.Extensions
{
    public static class UnityContainerExtension
    {
        public static IUnityContainer RegisterWorkflow(this IUnityContainer container)
        {
            if (!container.Resolve<IOptions<CamundaSettings>>().Value.IsEnabled)
            {
                return container;
            }

            container.RegisterFactory<Func<bool, string, IMonitoringRepository>>(
            c => new Func<bool, string, IMonitoringRepository>(
            (isLive, culture) =>
            {
                return new QpMonitoringRepository(container.Resolve<IConnectionProvider>(), container.Resolve<IArticleFormatter>(), isLive, culture);
                    }
                )
            );

            return container;
        }
    }
}
