using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Serialization;
using QA.Core.DPC.Loader;
using QA.ProductCatalog.TmForum.Services;
using Unity;
using Unity.Extension;

namespace QA.ProductCatalog.TmForum.Container
{
    public class TmfConfigurationExtension : UnityContainerExtension
    {
        public const string TmfItemIdentifier = "tmf";

        protected override void Initialize()
        {
            Container.RegisterFactory<IJsonProductService>(
                CreateTmfAwareFactory(
                    tmfinstanceFactory: (container) => container.Resolve<TmfProductService>(),
                    defaultFactory: (container) => container.Resolve<JsonProductService>()));

            Container.RegisterFactory<IProductDeserializer>(
                CreateTmfAwareFactory(
                    tmfinstanceFactory: (container) => container.Resolve<TmfProductDeserializer>(),
                    defaultFactory: (container) => container.Resolve<ProductDeserializer>()));

            Container.RegisterFactory<JsonProductServiceSettings>(
                CreateTmfAwareFactory(
                    tmfinstanceFactory: (container) =>
                    {
                        var accessor = container.Resolve<IHttpContextAccessor>();
                        var hasFieldsFilter = accessor.HttpContext.Request.Query.TryGetValue("fields", out StringValues fields);
                        var fieldsFilter = hasFieldsFilter
                            ? (ICollection<string>)new HashSet<string>(fields.ToArray(), StringComparer.OrdinalIgnoreCase)
                            : Array.Empty<string>();

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
        }

        private static Func<IUnityContainer, T> CreateTmfAwareFactory<T>(
            Func<IUnityContainer, T> tmfinstanceFactory,
            Func<IUnityContainer, T> defaultFactory)
        {
            return (container) =>
            {
                if (container.IsRegistered<IHttpContextAccessor>())
                {
                    var accessor = container.Resolve<IHttpContextAccessor>();

                    if (accessor.HttpContext?.Items.ContainsKey(TmfItemIdentifier) == true)
                    {
                        return tmfinstanceFactory(container);
                    }
                }

                return defaultFactory(container);
            };
        }
    }
}
