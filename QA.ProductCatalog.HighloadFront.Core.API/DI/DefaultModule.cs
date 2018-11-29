using System;
using Autofac;
using Autofac.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using QA.Core;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.DPC.Core.Helpers;
using QA.ProductCatalog.HighloadFront.Core.API.Helpers;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Options;
using QA.ProductCatalog.HighloadFront.PostProcessing;
using QA.ProductCatalog.Infrastructure;
using Service = QA.Core.DPC.QP.Models.Service;

namespace QA.ProductCatalog.HighloadFront.Core.API.DI
{
    internal class DefaultModule : Module
    {
        public IConfiguration Configuration { get; set; }

        public bool IsQpMode => !String.Equals(Configuration["Data:QpMode"], "false"
            , StringComparison.InvariantCultureIgnoreCase);
        
        public int SettingsContentId => Int32.TryParse(Configuration["Data:SettingsContentId"], out var result) ? result : 0;


        protected override void Load(ContainerBuilder builder)
        {

            builder.RegisterSingleton<ILogger>(n => new NLogLogger("NLog.config"));
            builder.RegisterSingleton<IHttpContextAccessor, HttpContextAccessor>();

            builder.RegisterScoped<ProductManager>();
            builder.RegisterType<ProductManager>().Named<ProductManager>("ForTask").ExternallyOwned();

            builder.RegisterScoped<SonicErrorDescriber>();
            
            builder.RegisterScoped<IProductStore, ElasticProductStore>();
            builder.RegisterType<ElasticProductStore>().Named<IProductStore>("ForTask").ExternallyOwned();

            builder.RegisterType<ProductImporter>().ExternallyOwned();
        

            builder.RegisterScoped<ICustomerProvider, CustomerProvider>();
            builder.RegisterScoped<IIdentityProvider>( c => new CoreIdentityProvider(
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

            builder.RegisterSingleton<ICustomerCodeInstanceCollection, CustomerCodeInstanceCollection>();
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
                                    (p, ctx) => p.ParameterType == typeof(IProductStore),
                                    (p, ctx) => ctx.ResolveNamed<IProductStore>("ForTask")
                                )
                            );

                            var importer = c.Resolve<ProductImporter>(
                                new TypedParameter(typeof(ProductManager), manager),
                                new NamedParameter("customerCode", c.Resolve<IIdentityProvider>().Identity.CustomerCode)
                            );

                            return new ReindexAllTask(importer, manager, c.Resolve<IElasticConfiguration>());
                        }
                    );
                }
            );

            builder.RegisterScoped(c => c.Resolve<CustomerCodeInstance>().CacheProvider);
            builder.RegisterScoped(c => c.Resolve<CustomerCodeTaskInstance>().TaskService);

            builder.RegisterScoped<IVersionedCacheProvider>(c => c.Resolve<IVersionedCacheProvider2>());

            if (IsQpMode)
            {
                if (SettingsContentId != 0)
                {
                    builder.RegisterScoped<ISettingsService>(c => new SettingsFromContentCoreService(
                        c.Resolve<IVersionedCacheProvider2>(),
                        c.Resolve<IConnectionProvider>(),
                        SettingsContentId
                        ));                    
                }
                else
                {
                    builder.RegisterScoped<ISettingsService, SettingsFromQpCoreService>();
                }
                builder.RegisterScoped<ISettingsService, SettingsFromQpCoreService>();
                builder.RegisterScoped<IContentProvider<ElasticIndex>, ElasticIndexProvider>();
                builder.RegisterScoped<IContentProvider<HighloadApiLimit>, HighloadApiLimitProvider>();
                builder.RegisterScoped<IContentProvider<HighloadApiUser>, HighloadApiUserProvider>();
                builder.RegisterScoped<IContentProvider<HighloadApiMethod>, HighloadApiMethodProvider>();                
                builder.RegisterScoped<IElasticConfiguration, QpElasticConfiguration>();
            }
            else
            {
                builder.RegisterScoped<IElasticConfiguration, JsonElasticConfiguration>();
            }



            builder.RegisterType<ArrayIndexer>().Named<IProductPostProcessor>("array");
            builder.RegisterType<DateIndexer>().Named<IProductPostProcessor>("date");
            builder.Register(c => new IndexerDecorator(new[]
            {
                c.ResolveNamed<IProductPostProcessor>("array"),
                c.ResolveNamed<IProductPostProcessor>("date")
            })).As<IProductPostProcessor>();

        }
    }
}