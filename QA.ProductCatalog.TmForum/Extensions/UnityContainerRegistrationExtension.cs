using Microsoft.Extensions.Options;
using QA.ProductCatalog.TmForum.Container;
using QA.ProductCatalog.TmForum.Models;
using Unity;

namespace QA.ProductCatalog.TmForum.Extensions;

public static class UnityContainerRegistrationExtension
{
    public static IUnityContainer RegisterTmForum(this IUnityContainer container)
    {
        if (container.Resolve<IOptions<TmfSettings>>().Value.IsEnabled)
        {
            container.AddNewExtension<TmfConfigurationExtension>();
        }

        return container;
    }
}
