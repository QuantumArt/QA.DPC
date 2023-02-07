using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Serialization;
using QA.Core.DPC.Loader;
using QA.ProductCatalog.TmForum.Interfaces;
using QA.ProductCatalog.TmForum.Services;
using Unity;
using Unity.Extension;

namespace QA.ProductCatalog.TmForum.Container
{
    public class TmfConfigurationExtension : UnityContainerExtension
    {
        public const string TmfItemIdentifier = "tmf";
        private static string[] _defaultTmfFieldsToSelect = new string[1] { "id" };

        protected override void Initialize()
        {
            IContainerRegistration[] toUnregister = Container.Registrations.Where(x =>
                x.RegisteredType == typeof(IJsonProductService)
                || x.RegisteredType == typeof(IProductDeserializer)
                || x.RegisteredType == typeof(JsonProductServiceSettings))
                .ToArray();

            foreach (IContainerRegistration unregister in toUnregister)
            {
                unregister.LifetimeManager.RemoveValue();
            }

            Container.RegisterFactory<IJsonProductService>(
                CreateTmfAwareFactory(
                    tmfInstanceFactory: (container) => container.Resolve<TmfProductService>(),
                    defaultFactory: (container) => container.Resolve<JsonProductService>()));

            Container.RegisterFactory<IProductDeserializer>(
                CreateTmfAwareFactory(
                    tmfInstanceFactory: (container) => container.Resolve<TmfProductDeserializer>(),
                    defaultFactory: (container) => container.Resolve<ProductDeserializer>()));

            Container.RegisterFactory<JsonProductServiceSettings>(
                CreateTmfAwareFactory(
                    tmfInstanceFactory: (container) =>
                    {
                        var accessor = container.Resolve<IHttpContextAccessor>();
                        var hasFieldsFilter = accessor.HttpContext.Request.Query.TryGetValue("fields", out StringValues fields);
                        ICollection<string> fieldsFilter = Array.Empty<string>();
                        if (hasFieldsFilter)
                        {
                            var filters = new HashSet<string>(fields.ToArray(), StringComparer.OrdinalIgnoreCase);
                            filters.UnionWith(_defaultTmfFieldsToSelect);
                            fieldsFilter = filters;
                        }

                        return new JsonProductServiceSettings
                        {
                            Fields = fieldsFilter,
                            WrapperName = string.Empty,
                            SerializerSettings = new Newtonsoft.Json.JsonSerializerSettings
                            {
                                ContractResolver = new CamelCasePropertyNamesContractResolver()
                            }
                        };
                    },
                    defaultFactory: (_) => new JsonProductServiceSettings()));

            Container.RegisterType<ITmfService, TmfService>();
        }

        private static Func<IUnityContainer, T> CreateTmfAwareFactory<T>(
            Func<IUnityContainer, T> tmfInstanceFactory,
            Func<IUnityContainer, T> defaultFactory)
        {
            return (container) =>
            {
                if (container.IsRegistered<IHttpContextAccessor>())
                {
                    var accessor = container.Resolve<IHttpContextAccessor>();

                    if (accessor.HttpContext?.Items.ContainsKey(TmfItemIdentifier) == true)
                    {
                        return tmfInstanceFactory(container);
                    }
                }

                return defaultFactory(container);
            };
        }
    }
}
