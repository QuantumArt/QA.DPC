using System;
using System.Runtime.Caching;
using Autofac;
using Microsoft.AspNetCore.Http;
using QA.Core;
using QA.Core.ProductCatalog.ActionsRunner;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.ProductCatalog.HighloadFront.Core.API.Helpers;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.HighloadFront.Importer;
using QA.ProductCatalog.HighloadFront.Infrastructure;
using QA.ProductCatalog.Infrastructure;
using Microsoft.Extensions.Configuration;
using QA.Core.Cache;
using QA.Core.Data;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.QP.Cache;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.DPC.Core.Helpers;

namespace QA.ProductCatalog.HighloadFront.Core.API
{
    internal class DefaultModule: Module
    {
        public IConfigurationRoot Configuration { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance<ObjectCache>(MemoryCache.Default);

            builder.RegisterSonic().AddProductStore<ElasticProductStore>();

            builder.Configure<HarvesterOptions>(Configuration.GetSection("Harvester"));

            builder.Configure<SonicElasticStoreOptions>(Configuration.GetSection("sonicElasticStore"));

            builder.RegisterSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.RegisterScoped<ISettingsService, SettingsFromQpService>();
            builder.RegisterScoped<ICustomerProvider, CustomerProvider>();
            builder.RegisterScoped<IIdentityProvider, CoreIdentityProvider>();

            ILogger logger = new NLogLogger("NLog.config");
            builder.RegisterInstance(logger);

            builder.RegisterSingleton<IVersionedCacheCoreProviderCollection, VersionedCacheCoreProviderCollection>();
            builder.RegisterScoped<IVersionedCacheProvider>(c => c.Resolve<IVersionedCacheCoreProviderCollection>().Get(c.GetCustomerCode()));

            builder.RegisterScoped<IConnectionProvider>(c => new ConnectionProvider(c.Resolve<ICustomerProvider>(), c.Resolve<IIdentityProvider>(), Service.HighloadAPI));

            builder.RegisterScoped<IContentProvider<ElasticIndex>, ElasticIndexProvider>();
            builder.RegisterScoped<IContentProvider<HighloadApiLimit>, HighloadApiLimitProvider>();
            builder.RegisterScoped<IContentProvider<HighloadApiUser>, HighloadApiUserProvider>();
            builder.RegisterScoped<CacheItemTracker, StructureCacheTracker>();
            builder.RegisterScoped<ICacheItemWatcher>(c =>
            {
                var a = new CustomerCacheItemWatcher(InvalidationMode.All, c.Resolve<IContentInvalidator>(), c.Resolve<IConnectionProvider>()); 
                a.AttachTracker(c.Resolve<CacheItemTracker>());
                return a;
            });

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

            builder.Register(c => new ArrayIndexer(GetOptions<SonicElasticStoreOptions>("sonicElasticStore").ArrayIndexingSettings)).Named<IProductPostProcessor>("array");
            builder.RegisterType<DateIndexer>().Named<IProductPostProcessor>("date");

            builder.Register(c => new IndexerDecorator(new[]
            {
                c.ResolveNamed<IProductPostProcessor>("array"),
                c.ResolveNamed<IProductPostProcessor>("date")
            })).As<IProductPostProcessor>();

            builder.RegisterType(typeof(ProductImporter)).InstancePerLifetimeScope();
        }


        private TOptions GetOptions<TOptions>(string name)
            where TOptions : class, new()
        {
            var section = Configuration.GetSection(name);
            var options = new ConfigureFromConfigurationOptions<TOptions>(section);
            var manager = new OptionsManager<TOptions>(new[] { options });
            return manager.Value;
        }


    }
}