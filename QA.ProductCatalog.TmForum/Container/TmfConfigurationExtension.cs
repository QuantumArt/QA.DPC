using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QA.Core.DPC.Loader;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.TmForum.Interfaces;
using QA.ProductCatalog.TmForum.Models;
using QA.ProductCatalog.TmForum.Providers;
using QA.ProductCatalog.TmForum.Services;
using Unity;
using Unity.Extension;

namespace QA.ProductCatalog.TmForum.Container
{
    public class TmfConfigurationExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Container.RegisterFactory<IJsonProductService>(
                CreateTmfAwareFactory(
                    tmfInstanceFactory: (container) => container.Resolve<TmfProductService>(),
                    defaultFactory: (container) => container.Resolve<JsonProductService>()));

            Container.RegisterFactory<IProductDeserializer>(
                CreateTmfAwareFactory(
                    tmfInstanceFactory: (container) => container.Resolve<TmfProductDeserializer>(),
                    defaultFactory: (container) => container.Resolve<ProductDeserializer>()));

            Container.RegisterFactory<IProductAddressProvider>((container) =>
            {
                if (container.IsRegistered<IHttpContextAccessor>())
                {
                    var accessor = container.Resolve<IHttpContextAccessor>();

                    if (accessor.HttpContext?.Items.ContainsKey(InternalTmfSettings.TmfItemIdentifier) == true)
                    {
                        return container.Resolve<DefaultProductAddressProvider>();
                    }
                }

                return container.Resolve<FakeProductAddressProvider>();
            });

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
                            filters.UnionWith(InternalTmfSettings.DefaultTmfFieldsToSelect);
                            fieldsFilter = filters;
                        }

                        JsonProductServiceSettings jsonSettings = new()
                        {
                            Fields = fieldsFilter,
                            SerializerSettings = new()
                            {
                                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                                Formatting = Formatting.Indented
                            }
                        };

                        if (accessor.HttpContext?.Items.ContainsKey(InternalTmfSettings.TmfItemIdentifier) == true
                            || accessor.HttpContext?.Items.ContainsKey(InternalTmfSettings.QueryResolverContextItemName) == true)
                        {
                            jsonSettings.WrapperName = string.Empty;
                        }

                        return jsonSettings;
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
                ISettingsService settings = container.Resolve<ISettingsService>();

                if (bool.TryParse(settings.GetSetting(SettingsTitles.TMF_ENABLED), out bool tmfEnabled) && tmfEnabled)
                {
                    return tmfInstanceFactory(container);
                }

                return defaultFactory(container);
            };
        }
    }
}
