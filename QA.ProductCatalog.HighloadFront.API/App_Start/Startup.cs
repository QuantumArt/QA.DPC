using System;
using System.Reflection;
using System.Runtime.Caching;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Autofac;
using Autofac.Integration.WebApi;
using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Nest;
using Owin;
using QA.Configuration;
using QA.Core;
using QA.Core.ProductCatalog.ActionsRunner;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.ProductCatalog.HighloadFront.App_Core;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.HighloadFront.Filters;
using QA.ProductCatalog.HighloadFront.Importer;
using QA.ProductCatalog.HighloadFront.Infrastructure;
using QA.ProductCatalog.HighloadFront.Models;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.HighloadFront.Importer.DpcServiceReference;

namespace QA.ProductCatalog.HighloadFront
{
    public class Startup
    {
        private IConfiguration _config { get; } = new ConfigurationBuilder()
            .AddJsonFile("Config.json")
            .Build();

        // Invoked once at startup to configure your application.
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            config.Formatters.Remove(config.Formatters.XmlFormatter);

            var containerBuilder = new ContainerBuilder();
            var container = DIConfiguration(containerBuilder);
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            //TestHandler._dependencyResolver = config.DependencyResolver;

            app.UseAutofacMiddleware(container);

            app.Use<FixContentTypeHeader>();

            app.UseAutofacWebApi(config);

            config.Services.Add(typeof(IExceptionLogger), new GlobalExceptionLogger(container.Resolve<ILogger>()));

            app.UseWebApi(config);

            //containerBuilder.RegisterWebApiFilterProvider(config);

            System.Threading.Tasks.Task.Run((Action)container.Resolve<ITasksRunner>().Run);
        }

        public IContainer DIConfiguration(ContainerBuilder builder)
        {
            builder.RegisterInstance<ObjectCache>(MemoryCache.Default);

            builder.RegisterSonic().AddProductStore<ElasticProductStore>();

            builder.Configure<HarvesterOptions>(_config.GetSection("Harvester"));

            builder.Configure<SonicElasticStoreOptions>(_config.GetSection("sonicElasticStore"));

            foreach (var x in new[] { "" })
            {
                var client = GetElasticClient(x, x);
                var key = GetKey(x, x);
                builder.RegisterInstance<IElasticClient>(client).Named<IElasticClient>(key);

                builder.Register<Func<string, string, IElasticClient>>(r => (a ,b) => r.ResolveNamed<IElasticClient>(GetKey(a, b)));
            }

            builder.RegisterSingleton<IElasticClient>(context =>
            {
                var node = new Uri(_config["Data:Elastic:Adress"]);

                var connectionPool = new SingleNodeConnectionPool(node);

                var settings = new ConnectionSettings(connectionPool, s => new JsonNetSerializer(s).EnableStreamResponse())
                                .DefaultIndex(_config["Data:Elastic:Index"])
                                .DisableDirectStreaming()
                                //.EnableTrace()
                                .ThrowExceptions();
                
                return new ElasticClient(settings);
            });


            builder.RegisterType<TasksRunner>().As<ITasksRunner>().SingleInstance();

            builder.RegisterType<InmemoryTaskService>().As<ITaskService>().SingleInstance();

            builder.Register<Func<ITaskService>>(c =>
            {
                var taskService = c.Resolve<ITaskService>();

                return () => taskService;

            }).SingleInstance();

            builder.Register<Func<string, int, ITask>>(c => { var reindexTask = c.ResolveNamed<ITask>("ReindexAllTask"); return (key, userId) => key == "ReindexAllTask" ? reindexTask : null; }).SingleInstance();

            builder.RegisterType<ReindexAllTask>().Named<ITask>("ReindexAllTask");

            builder.RegisterInstance(new IndexOperationSyncer());


            builder.RegisterInstance<ILogger>(new NLogLogger("NLog.config"));

            builder.RegisterScoped<IDpcService, DpcServiceClient>();

            builder.RegisterType<ConfigurationService>().As<IConfigurationService>().SingleInstance();

            builder.Register(c => new ArrayIndexer(c.Resolve<IConfigurationService>().GetConfiguration<ArrayIndexingSettings[]>())).Named<IProductPostProcessor>("array");
            builder.RegisterType<DateIndexer>().Named<IProductPostProcessor>("date");

            builder.Register(c => new IndexerDecorator(new[]
            {
                c.ResolveNamed<IProductPostProcessor>("array"),
                c.ResolveNamed<IProductPostProcessor>("date")
            })).As<IProductPostProcessor>();

            builder.RegisterType(typeof(ProductImporter)).InstancePerLifetimeScope();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());


            builder.Configure<RateLimitOptions>(_config.GetSection("RateLimits"));

            builder.Configure<Users>(_config.GetSection("Users"));


            return builder.Build();
        }

        private string GetKey(string index, string address)
        {
            return $"{index}-{address}";
        }

        private IElasticClient GetElasticClient(string index, string address)
        {
            var node = new Uri(_config[address]);

            var connectionPool = new SingleNodeConnectionPool(node);

            var settings = new ConnectionSettings(connectionPool, s => new JsonNetSerializer(s).EnableStreamResponse())
                            .DefaultIndex(index)
                            .DisableDirectStreaming()
                            //.EnableTrace()
                            .ThrowExceptions();

            return new ElasticClient(settings);
        }
    }
}