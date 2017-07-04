using System;
using System.Configuration;
using System.Globalization;
using Microsoft.Practices.Unity;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration.DAL;

namespace QA.ProductCatalog.Integration.Configuration
{
	public class IntegrationContainerConfiguration : UnityContainerExtension
	{
		protected override void Initialize()
		{

		}


	    public static void RegisterQpMonitoring(UnityContainer container)
	    {
	        container.RegisterType<Func<bool, IConsumerMonitoringService>>(
	            new InjectionFactory(x =>
	                new Func<bool, IConsumerMonitoringService>(
	                    isLive =>
	                    {
	                        var repo = new QpMonitoringRepository(container.Resolve<IConnectionProvider>(), isLive);
	                        return new ConsumerMonitoringService(repo);
	                    }
	                )
	            )
	        );

	        container.RegisterType<Func<bool, CultureInfo, IConsumerMonitoringService>>(
	            new InjectionFactory(c => new Func<bool, CultureInfo, IConsumerMonitoringService>(
	                (isLive, culture) =>
	                {
	                    var repo = new QpMonitoringRepository(container.Resolve<IConnectionProvider>(), isLive, culture.Name);
	                    return new ConsumerMonitoringService(repo);
	                }
	            ))
	        );

	        container.RegisterType<IConsumerMonitoringService>(
	            new InjectionFactory(c => c.Resolve<Func<bool, IConsumerMonitoringService>>().Invoke(true)));
        }

        public static void RegisterNonQpMonitoring(UnityContainer container)
	    {
	        foreach (ConnectionStringSettings cnn in ConfigurationManager.ConnectionStrings)
	        {
	            if (cnn.Name.StartsWith("consumer_monitoring"))
	            {
	                container.RegisterInstance<IConnectionProvider>(cnn.Name,
	                    new ExplicitConnectionProvider(ConfigurationManager.ConnectionStrings[cnn.Name]
	                        .ConnectionString)
	                );
	            }
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
