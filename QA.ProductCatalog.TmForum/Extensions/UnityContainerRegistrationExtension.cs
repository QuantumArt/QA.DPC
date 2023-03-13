using QA.ProductCatalog.TmForum.Container;
using Unity;

namespace QA.ProductCatalog.TmForum.Extensions
{
    public static class UnityContainerRegistrationExtension
    {
        public static IUnityContainer RegisterTmForum(this IUnityContainer container)
        {
            container.AddNewExtension<TmfConfigurationExtension>();

            return container;
        }
    }
}
