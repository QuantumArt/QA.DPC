using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.Core.ProductCatalog.ActionsRunner;
using QA.DotNetCore.Caching.Interfaces;
using QA.DPC.Core.Helpers;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.ContentProviders.Deprecated;
using QA.ProductCatalog.HighloadFront.Core.API.Helpers;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Options;
using QA.ProductCatalog.HighloadFront.PostProcessing;
using QA.ProductCatalog.HighloadFront.Validation;
using Service = QA.Core.DPC.QP.Models.Service;

namespace QA.ProductCatalog.HighloadFront.Core.API.DI
{
    internal class DefaultModule : Module
    {
        public IConfiguration Configuration { get; set; }

        public bool IsQpMode => !string.Equals(Configuration["Data:QpMode"], "false", StringComparison.InvariantCultureIgnoreCase);

        public int SettingsContentId => int.TryParse(Configuration["Data:SettingsContentId"], out var result) ? result : 0;

        protected override void Load(ContainerBuilder builder)
        {

            builder.RegisterSingleton<ILogger>(n => new NLogLogger("NLog.config"));
            builder.RegisterSingleton<IHttpContextAccessor, HttpContextAccessor>();

            builder.RegisterScoped<ProductManager>();
            builder.RegisterType<ProductManager>().Named<ProductManager>("ForTask").ExternallyOwned();

            builder.RegisterScoped<SonicErrorDescriber>();

            builder.RegisterScoped<Func<string, IProductStore>>(c =>
            {
                var context = c.Resolve<IComponentContext>();
                return version => context.ResolveNamed<IProductStore>(version);
            });
            builder.RegisterScoped<IProductStoreFactory, ProductStoreFactory>();
            builder.RegisterType<ProductStoreFactory>().Named<IProductStoreFactory>("ForTask").ExternallyOwned();
            builder.RegisterType<ElasticProductStore>().Named<IProductStore>("5.*").ExternallyOwned();
            builder.RegisterType<ElasticProductStore_6>().Named<IProductStore>("6.*").ExternallyOwned();
            builder.RegisterType<ElasticProductStore_8>().Named<IProductStore>("8.*").ExternallyOwned();
            builder.RegisterType<ProductImporter>().ExternallyOwned();

            builder.RegisterScoped<ICustomerProvider, CustomerProvider>();
            builder.RegisterScoped<IIdentityProvider>(c => new CoreIdentityFixedProvider(
                c.Resolve<IHttpContextAccessor>(),
                c.Resolve<DataOptions>().FixedCustomerCode
            ));
            builder.RegisterScoped<IConnectionProvider>(c =>
            {
                var fcnn = c.Resolve<DataOptions>().FixedConnectionString;
                if (!string.IsNullOrEmpty(fcnn) || !IsQpMode)
                    return new ExplicitConnectionProvider(fcnn);

                return new ConnectionProvider(
                    c.Resolve<ICustomerProvider>(),
                    c.Resolve<IIdentityProvider>(),
                    Service.HighloadAPI
                );
            });

            builder.RegisterSingleton<ICustomerCodeInstanceCollection>(c => new CustomerCodeInstanceCollection(c.Resolve<ILogger>(), c.Resolve<TaskRunnerDelays>(), null));
            builder.RegisterScoped(c => c.Resolve<ICustomerCodeInstanceCollection>().Get(
                c.Resolve<IIdentityProvider>(),
                c.Resolve<IConnectionProvider>()
                )
            );

            builder.RegisterScoped(c =>
                {
                    return c.Resolve<ICustomerCodeInstanceCollection>().Get(
                        c.Resolve<IIdentityProvider>(), () =>
                        {
                            var manager = c.ResolveNamed<ProductManager>("ForTask",
                                new ResolvedParameter(
                                    (p, ctx) => p.ParameterType == typeof(IProductStoreFactory),
                                    (p, ctx) => ctx.ResolveNamed<IProductStoreFactory>("ForTask")
                                )
                            );

                            var importer = c.Resolve<ProductImporter>(
                                new TypedParameter(typeof(ProductManager), manager),
                                new NamedParameter("customerCode", c.Resolve<IIdentityProvider>().Identity.CustomerCode)
                            );

                            return new ReindexAllTask(importer, manager, c.Resolve<ElasticConfiguration>(), new Dictionary<string, IProductStore>() {
                                    { "5.*", c.ResolveNamed<IProductStore>("5.*")},
                                    { "6.*", c.ResolveNamed<IProductStore>("6.*") },
                                    { "8.*", c.ResolveNamed<IProductStore>("8.*") }
                            });
                        }
                    );
                }
            );

            builder.RegisterScoped(c => c.Resolve<CustomerCodeInstance>().CacheProvider);
            builder.RegisterScoped(c => c.Resolve<CustomerCodeTaskInstance>().TaskService);

            if (IsQpMode)
            {
                if (SettingsContentId != 0)
                {
                    builder.RegisterScoped<ISettingsService>(c => new SettingsFromContentCoreService(
                        c.Resolve<IConnectionProvider>(),
                        c.Resolve<ICacheProvider>(),
                        SettingsContentId
                        ));
                }
                else
                {
                    builder.RegisterScoped<ISettingsService, SettingsFromQpCoreService>();
                }
                
                builder.RegisterScoped<IContentProvider<ElasticIndex>, ElasticIndexProvider>();
                builder.RegisterScoped<IContentProvider<HighloadApiLimit>, HighloadApiLimitProvider>();
                builder.RegisterScoped<IContentProvider<HighloadApiUser>, HighloadApiUserProvider>();
                builder.RegisterScoped<IContentProvider<HighloadApiMethod>, HighloadApiMethodProvider>();
                builder.RegisterScoped<ElasticConfiguration, QpElasticConfiguration>();
            }
            else
            {
                builder.RegisterScoped<ElasticConfiguration, JsonElasticConfiguration>();
            }

            builder.RegisterScoped<IProductReadPostProcessor, ContentProcessor>();
            builder.RegisterScoped<IProductReadExpandPostProcessor, ExpandReadProcessor>();
            builder.RegisterScoped<IProductWriteExpandPostProcessor, ExpandWriteProcessor>();

            builder.RegisterType<ArrayIndexer>().Named<IProductWritePostProcessor>("array");
            builder.RegisterType<DateIndexer>().Named<IProductWritePostProcessor>("date");
            builder.Register(c => new IndexerDecorator(new[]
            {
                c.ResolveNamed<IProductWritePostProcessor>("array"),
                c.ResolveNamed<IProductWritePostProcessor>("date")
            })).As<IProductWritePostProcessor>();

            builder.RegisterScoped<ProductsOptionsCommonValidationHelper>();
        }
    }
}