using System;
using System.Linq;
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

            //System.Threading.Tasks.Task.Run((Action)container.Resolve<ITasksRunner>().Run);
        }

        public IContainer DIConfiguration(ContainerBuilder builder)
        {
            builder.RegisterInstance<ObjectCache>(MemoryCache.Default);

            builder.RegisterSonic().AddProductStore<ElasticProductStore>();

            builder.Configure<HarvesterOptions>(_config.GetSection("Harvester"));

            builder.Configure<DataOptions>(_config.GetSection("Data"));

            builder.Configure<SonicElasticStoreOptions>(_config.GetSection("sonicElasticStore"));

            ILogger logger = new NLogLogger("NLog.config");
            builder.RegisterInstance<ILogger>(logger);

            var elasticOptions = GetOptions<DataOptions>("Data").Elastic;

            var elasticClientMap = elasticOptions.ToDictionary(
                    option => GetElasticKey(option.Language, option.State),
                    option => GetElasticClient(option.Index, option.Adress, option.Timeout, logger, option.DoTrace)
                );

            var syncerMap = elasticOptions.ToDictionary(
                    option => GetElasticKey(option.Language, option.State),
                    option => new IndexOperationSyncer()
                );

            var defaultElasticOption = elasticOptions.FirstOrDefault(option => option.IsDefault);

            if (defaultElasticOption != null)
            {
                var defaultKey = GetElasticKey(null, null);
                var actualKey = GetElasticKey(defaultElasticOption.Language, defaultElasticOption.State);
                elasticClientMap[defaultKey] = elasticClientMap[actualKey];
                builder.RegisterSingleton<IElasticClient>(c => elasticClientMap[actualKey]);
                builder.RegisterInstance(syncerMap[actualKey]);
                builder.Register<IDpcService>(c => new DpcServiceClient(actualKey));
            }

            builder.Register<Func<string, string, IElasticClient>>(c => (language, state) => elasticClientMap[GetElasticKey(language, state)]);
            builder.Register<Func<string, string, IDpcService>>(c => (language, state) => new DpcServiceClient(GetElasticKey(language, state)));
            builder.Register<Func<string, string, IndexOperationSyncer>>(c => (language, state) => syncerMap[GetElasticKey(language, state)]);

            builder.RegisterType<TasksRunner>().As<ITasksRunner>().SingleInstance();

            builder.RegisterType<InmemoryTaskService>().As<ITaskService>().SingleInstance();

            builder.Register<Func<ITaskService>>(c =>
            {
                var taskService = c.Resolve<ITaskService>();

                return () => taskService;

            }).SingleInstance();

            builder.Register<Func<string, int, ITask>>(c => { var reindexTask = c.ResolveNamed<ITask>("ReindexAllTask"); return (key, userId) => key == "ReindexAllTask" ? reindexTask : null; }).SingleInstance();

            builder.RegisterType<ReindexAllTask>().Named<ITask>("ReindexAllTask");

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

        private TOptions GetOptions<TOptions>(string name)
            where TOptions : class, new()
        {
            var section =_config.GetSection(name);
            var options = new ConfigureFromConfigurationOptions<TOptions>(section);
            var manager = new OptionsManager<TOptions>(new[] { options });
            return manager.Value;
        }

        private string GetElasticKey(string language, string state)
        {
            if (language == null && state == null)
            {
                return "default";
            }
            else
            {
                return $"{language}-{state}";
            }
        }

        private IElasticClient GetElasticClient(string index, string address, int timeout, ILogger logger, bool doTrace)
        {
            var node = new Uri(address);

            var connectionPool = new SingleNodeConnectionPool(node);

            var settings = new ConnectionSettings(connectionPool, s => new JsonNetSerializer(s).EnableStreamResponse())
                            .DefaultIndex(index)
                            .RequestTimeout(TimeSpan.FromSeconds(timeout))
                            .DisableDirectStreaming()
                            .EnableTrace(msg => logger.Log(() => msg, EventLevel.Trace), doTrace)
                            //.EnableTrace(msg => Debug.WriteLine(msg), doTrace)
                            .ThrowExceptions();

            return new ElasticClient(settings);
        }
    }
}