using System;
using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using QA.Core;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.Core.ProductCatalog.ActionsRunner;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.DPC.Core.Helpers;
using QA.ProductCatalog.HighloadFront.Core.API.Helpers;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Options;
using QA.ProductCatalog.HighloadFront.PostProcessing;
using QA.ProductCatalog.Infrastructure;

namespace QA.ProductCatalog.HighloadFront.Core.API.DI
{
    internal class DefaultModule: Module
    {
        public IConfigurationRoot Configuration { get; set; }

        protected override void Load(ContainerBuilder builder)
        {

            builder.RegisterSingleton<ILogger>(n => new NLogLogger("NLog.config"));
            builder.RegisterSingleton<IHttpContextAccessor, HttpContextAccessor>();

            builder.RegisterScoped<ProductManager>();
            builder.RegisterScoped<SonicErrorDescriber>();
            
            builder.RegisterScoped<IProductStore, ElasticProductStore>();
            builder.RegisterScoped<ICustomerProvider, CustomerProvider>();
            builder.RegisterScoped<IIdentityProvider>(c => new CoreIdentityProvider(c.Resolve<IHttpContextAccessor>(), c.Resolve<IOptions<DataOptions>>().Value.FixedCustomerCode));
            builder.RegisterScoped<IConnectionProvider>(c => new ConnectionProvider(c.Resolve<ICustomerProvider>(), c.Resolve<IIdentityProvider>(), Service.HighloadAPI));

            builder.RegisterSingleton<IVersionedCacheCoreProviderCollection, VersionedCacheCoreProviderCollection>();
            builder.RegisterScoped<IVersionedCacheProvider2>(c => c.Resolve<IVersionedCacheCoreProviderCollection>().Get(
                c.Resolve<IIdentityProvider>(), c.Resolve<IConnectionProvider>()
            ));
            builder.RegisterScoped<IVersionedCacheProvider>(c => c.Resolve<IVersionedCacheProvider2>());

            builder.RegisterScoped<ISettingsService, SettingsFromQpCoreService>();
            builder.RegisterScoped<IContentProvider<ElasticIndex>, ElasticIndexProvider>();
            builder.RegisterScoped<IContentProvider<HighloadApiLimit>, HighloadApiLimitProvider>();
            builder.RegisterScoped<IContentProvider<HighloadApiUser>, HighloadApiUserProvider>();
            builder.RegisterScoped<IElasticConfiguration, QpElasticConfiguration>();

            builder.RegisterType<TasksRunner>().As<ITasksRunner>().SingleInstance();
            builder.RegisterType<InmemoryTaskService>().As<ITaskService>().SingleInstance();
            builder.Register<Func<ITaskService>>(c =>
            {
                var taskService = c.Resolve<ITaskService>();
                return () => taskService;
            }).SingleInstance();

            builder.Register<Func<string, int, ITask>>(c => { var reindexTask = c.ResolveNamed<ITask>("ReindexAllTask"); return (key, userId) => key == "ReindexAllTask" ? reindexTask : null; }).SingleInstance();
            builder.RegisterType<ReindexAllTask>().Named<ITask>("ReindexAllTask");

            builder.RegisterType<ArrayIndexer>().Named<IProductPostProcessor>("array");
            builder.RegisterType<DateIndexer>().Named<IProductPostProcessor>("date");
            builder.Register(c => new IndexerDecorator(new[]
            {
                c.ResolveNamed<IProductPostProcessor>("array"),
                c.ResolveNamed<IProductPostProcessor>("date")
            })).As<IProductPostProcessor>();

            builder.RegisterType(typeof(ProductImporter)).InstancePerLifetimeScope();
        }
    }
}