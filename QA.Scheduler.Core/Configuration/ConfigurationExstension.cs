using System;
using QA.Core.Logger;
using QA.Scheduler.API.Models;
using QA.Scheduler.API.Services;
using QA.Scheduler.Core.Schedules;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace QA.Scheduler.Core.Configuration
{
    public static class ConfigurationExstension
	{
		public const string DefaultScheduleName = "NullSchedule";

		public static void RegisterProcessor<T>(this IUnityContainer container, string service, string schedule, ITypeLifetimeManager lifetimeManager)
		where T : IProcessor
		{
			string name = Guid.NewGuid().ToString();
			container.RegisterType<IProcessor, T>(name, lifetimeManager);

			var descriptor = new ProcessorDescriptor(name, service, schedule);
			container.RegisterInstance<ProcessorDescriptor>(descriptor.Processor, descriptor);
		}

		public static void RegisterProcessor<T>(this IUnityContainer container, string service, string schedule)
		where T : IProcessor
		{
			container.RegisterProcessor<T>(service, schedule, new TransientLifetimeManager());
		}

		public static void RegisterProcessor<T>(this IUnityContainer container, string service, TimeSpan interval, ITypeLifetimeManager lifetimeManager)
		where T : IProcessor
		{
			string schedule = Guid.NewGuid().ToString();
			container.RegisterSchedule(schedule, interval);
			container.RegisterProcessor<T>(service, schedule, lifetimeManager);
		}

		public static void RegisterProcessor<T>(this IUnityContainer container, string service, TimeSpan interval)
		where T : IProcessor
		{
			container.RegisterProcessor<T>(service, interval, new TransientLifetimeManager());
		}

		public static void RegisterProcessor<TProcessor, TSchedule>(this IUnityContainer container, string service, ITypeLifetimeManager lifetimeManager)
			where TProcessor : IProcessor
			where TSchedule : ISchedule
		{

			string schedule = Guid.NewGuid().ToString();
			container.RegisterType<ISchedule, TSchedule>(schedule, new HierarchicalLifetimeManager());
			container.RegisterProcessor<TProcessor>(service, schedule, lifetimeManager);
		}

		public static void RegisterProcessor<TProcessor, TSchedule>(this IUnityContainer container, string service)
			where TProcessor : IProcessor
			where TSchedule : ISchedule
		{
			container.RegisterProcessor<TProcessor, TSchedule>(service, new TransientLifetimeManager());
		}

		public static void RegisterProcessor<T>(this IUnityContainer container, string service)
		where T : IProcessor
		{
			container.RegisterProcessor<T>(service, DefaultScheduleName);
		}

		public static void RegisterSchedule<T>(this IUnityContainer container, string name)
			where T : ISchedule
		{
			container.RegisterSchedule<T>(name, new HierarchicalLifetimeManager());
		}

		public static void RegisterSchedule<T>(this IUnityContainer container, string name, ITypeLifetimeManager lifetimeManager)
			where T : ISchedule
		{
			container.RegisterType<ISchedule, T>(name, lifetimeManager);
		}

		public static void RegisterSchedule(this IUnityContainer container, string name, TimeSpan interval)
		{
			container.RegisterSchedule(name, interval, new HierarchicalLifetimeManager());
		}

		public static void RegisterSchedule(this IUnityContainer container, string name, TimeSpan interval, IFactoryLifetimeManager lifetimeManager)
		{
			container.RegisterFactory<ISchedule>(name, c => new IntervalSchedule(interval), lifetimeManager);
		}

		public static void RegisterService(this IUnityContainer container, string key, string name, string description)
		{
			var descriptor = new ServiceDescriptor(key, name, description);
			container.RegisterInstance<ServiceDescriptor>(descriptor.Key, descriptor);
			container.RegisterFactory<ILogger>(key, c => new NLogLogger(key + ".log.config"), new HierarchicalLifetimeManager());
		}
	}
}
