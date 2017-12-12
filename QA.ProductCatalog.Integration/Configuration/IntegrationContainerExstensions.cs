using Microsoft.Practices.Unity;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration.DAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;

namespace QA.ProductCatalog.Integration.Configuration
{
    public static class IntegrationContainerExstensions
    {
        public static void RegisterQpMonitoring(this IUnityContainer container)
        {
            container.RegisterType<Func<bool, IConsumerMonitoringService>>(
                new InjectionFactory(x =>
                    new Func<bool, IConsumerMonitoringService>(
                        isLive =>
                        {
                            var repo = new QpMonitoringRepository(container.Resolve<IConnectionProvider>(), container.Resolve<IArticleFormatter>(), isLive);
                            return new ConsumerMonitoringService(repo);
                        }
                    )
                )
            );

            container.RegisterType<Func<bool, CultureInfo, IConsumerMonitoringService>>(
                new InjectionFactory(c => new Func<bool, CultureInfo, IConsumerMonitoringService>(
                    (isLive, culture) =>
                    {
                        var repo = new QpMonitoringRepository(container.Resolve<IConnectionProvider>(), container.Resolve<IArticleFormatter>(), isLive, culture.Name);
                        return new ConsumerMonitoringService(repo);
                    }
                ))
            );

            container.RegisterType<IConsumerMonitoringService>(
                new InjectionFactory(c => c.Resolve<Func<bool, IConsumerMonitoringService>>().Invoke(true)));

            container.RegisterType<IList<IConsumerMonitoringService>>(
                new InjectionFactory(c =>
                    new[] { false, true }
                    .SelectMany(isLive =>
                        c.Resolve<IProductLocalizationService>()
                        .GetCultures()
                        .Select(culture => c.Resolve<Func<bool, CultureInfo, IConsumerMonitoringService>>()(isLive, culture))
                    ).ToList()));
        }

        public static void RegisterNonQpMonitoring(this IUnityContainer container)
        {
            var monitoringConnections = ConfigurationManager.ConnectionStrings
                .OfType<ConnectionStringSettings>()
                .Where(c => c.Name.StartsWith("consumer_monitoring"))
                .ToArray();

            foreach (ConnectionStringSettings cnn in monitoringConnections)
            {
                container.RegisterInstance<IConnectionProvider>(cnn.Name, new ExplicitConnectionProvider(cnn.ConnectionString));
            }

            container.RegisterType<Func<bool, IConsumerMonitoringService>>(
                new InjectionFactory(x =>
                    new Func<bool, IConsumerMonitoringService>(
                        isLive =>
                        {
                            var key = isLive ? "consumer_monitoring" : "consumer_monitoringStage";
                            var repo = new MonitoringRepository(container.Resolve<IConnectionProvider>(key));
                            return new ConsumerMonitoringService(repo);
                        }
                    )
                )
            );

            container.RegisterType<IList<IConsumerMonitoringService>>(new InjectionFactory(x =>
                monitoringConnections
                .Select(cnn => new ConsumerMonitoringService(new MonitoringRepository(x.Resolve<IConnectionProvider>(cnn.Name))) as IConsumerMonitoringService)
                .ToList())
            );

            container.RegisterType<Func<bool, CultureInfo, IConsumerMonitoringService>>(
                new InjectionFactory(c => new Func<bool, CultureInfo, IConsumerMonitoringService>(
                    (isLive, culture) =>
                    {
                        var key = (isLive ? "consumer_monitoring" : "consumer_monitoringStage") + culture.Name;
                        var repo = new MonitoringRepository(container.Resolve<IConnectionProvider>(key));
                        return new ConsumerMonitoringService(repo);
                    }
                ))
            );

            container.RegisterType<IConsumerMonitoringService>(
                new InjectionFactory(c => c.Resolve<Func<bool, IConsumerMonitoringService>>().Invoke(true)));
        }
    }
}
