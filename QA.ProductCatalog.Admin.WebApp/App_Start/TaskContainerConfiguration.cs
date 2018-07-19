using System;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.ProductCatalog.Admin.WebApp.Core.Adapters;
using QA.ProductCatalog.Infrastructure;
using Unity;
using Unity.Extension;
using Unity.Injection;

namespace QA.ProductCatalog.Admin.WebApp
{
    public class TaskContainerConfiguration : UnityContainerExtension
	{
		protected override void Initialize()
		{
			Container.RegisterType<Func<string, string, IAction>>(new InjectionFactory(c => new Func<string, string, IAction>(
				(actionKey, adapterKey) =>
				{
					if (c.IsRegistered<ActionAdapterBase>(adapterKey) && c.IsRegistered<ITask>(actionKey))
					{
						var adapter = c.Resolve<ActionAdapterBase>(adapterKey);
						adapter.TaskKey = actionKey;
						return adapter;
					}
					else
					{
						return c.Resolve<IAction>(actionKey);
					}
				})));

			var a = typeof(TaskContainerConfiguration).Assembly;
			foreach (var t in a.GetExportedTypes())
			{
				if (typeof(ActionAdapterBase).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
				{
					Container.RegisterType (typeof(ActionAdapterBase), t, t.Name);
				}
			}
		}
	}
}