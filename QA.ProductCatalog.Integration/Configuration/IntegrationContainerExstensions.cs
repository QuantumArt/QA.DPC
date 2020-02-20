using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration.DAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Options;
using QA.Core.DPC.QP.Models;
using Unity;
using Unity.Injection;

namespace QA.ProductCatalog.Integration.Configuration
{
    public static class IntegrationContainerExstensions
    {
        public static void RegisterQpMonitoring(this IUnityContainer container)
        {
            container.RegisterFactory<Func<bool, IConsumerMonitoringService>>(
                x =>
                    new Func<bool, IConsumerMonitoringService>(
                        isLive =>
                        {
                            var repo = new QpMonitoringRepository(container.Resolve<IConnectionProvider>(), container.Resolve<IArticleFormatter>(), isLive);
                            return new ConsumerMonitoringService(repo);
                        }
                    )
            );

            container.RegisterFactory<Func<bool, CultureInfo, IConsumerMonitoringService>>(
                c => new Func<bool, CultureInfo, IConsumerMonitoringService>(
                    (isLive, culture) =>
                    {
                        var repo = new QpMonitoringRepository(container.Resolve<IConnectionProvider>(), container.Resolve<IArticleFormatter>(), isLive, culture.Name);
                        return new ConsumerMonitoringService(repo);
                    }
                )
            );

            container.RegisterFactory<IConsumerMonitoringService>(
               c => c.Resolve<Func<bool, IConsumerMonitoringService>>().Invoke(true));

            container.RegisterFactory<IList<IConsumerMonitoringService>>(
                c =>
                    new[] { false, true }
                    .SelectMany(isLive =>
                        c.Resolve<IProductLocalizationService>()
                        .GetCultures()
                        .Select(culture => c.Resolve<Func<bool, CultureInfo, IConsumerMonitoringService>>()(isLive, culture))
                    ).ToList());
        }

        public static void RegisterNonQpMonitoring(this IUnityContainer container)
        {
            IOptions<ConnectionProperties> cnnProps = container.Resolve<IOptions<ConnectionProperties>>();
            if (!string.IsNullOrEmpty(cnnProps.Value.LiveMonitoringConnectionString))
            {
                container.RegisterInstance<IConnectionProvider>("consumer_monitoring",
                    new ExplicitConnectionProvider(cnnProps.Value.LiveMonitoringConnectionString));
            }
            if (!string.IsNullOrEmpty(cnnProps.Value.StageMonitoringConnectionString))
            {
                container.RegisterInstance<IConnectionProvider>("consumer_monitoringStage",
                    new ExplicitConnectionProvider(cnnProps.Value.StageMonitoringConnectionString));
            }

            container.RegisterFactory<Func<bool, IConsumerMonitoringService>>(
                x =>
                    new Func<bool, IConsumerMonitoringService>(
                        isLive =>
                        {
                            var key = isLive ? "consumer_monitoring" : "consumer_monitoringStage";
                            var repo = new MonitoringRepository(container.Resolve<IConnectionProvider>(key));
                            return new ConsumerMonitoringService(repo);
                        }
                    )
            );

            container.RegisterFactory<IList<IConsumerMonitoringService>>(x => new List<IConsumerMonitoringService>
            {
                new ConsumerMonitoringService(new MonitoringRepository(
                    x.Resolve<IConnectionProvider>("consumer_monitoring"))),
                new ConsumerMonitoringService(new MonitoringRepository(
                    x.Resolve<IConnectionProvider>("consumer_monitoringStage")))
            });

            container.RegisterFactory<Func<bool, CultureInfo, IConsumerMonitoringService>>(
                c => new Func<bool, CultureInfo, IConsumerMonitoringService>(
                    (isLive, culture) =>
                    {
                        var key = (isLive ? "consumer_monitoring" : "consumer_monitoringStage") + culture.Name;
                        var repo = new MonitoringRepository(container.Resolve<IConnectionProvider>(key));
                        return new ConsumerMonitoringService(repo);
                    }
                )
            );

            container.RegisterFactory<IConsumerMonitoringService>(
                c => c.Resolve<Func<bool, IConsumerMonitoringService>>().Invoke(true));
        }
    }
}
