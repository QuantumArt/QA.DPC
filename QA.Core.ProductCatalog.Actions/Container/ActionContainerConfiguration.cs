using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.ProductCatalog.Infrastructure;
using System;
using QA.Core.Logger;
using QA.ProductCatalog.ContentProviders;
using Unity;
using Unity.Extension;
using Unity.Injection;

namespace QA.Core.ProductCatalog.Actions.Container
{
    public class ActionContainerConfiguration : UnityContainerExtension
	{
		protected override void Initialize()
		{
            Container.RegisterInstance<ILogger>(new NLogLogger("NLogClient.config"));			
			Container.RegisterFactory<Func<string, IAction>>(c => new Func<string, IAction>(name => c.Resolve<IAction>(name)));
			Container.RegisterFactory<Func<string, ITask>>(c => new Func<string, ITask>(name => c.Resolve<ITask>(name)));

			var a = typeof(ActionContainerConfiguration).Assembly;
			foreach (var t in a.GetExportedTypes())
			{
				if (typeof(IAction).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
				{
					Container.RegisterType(typeof(IAction), t, t.Name);
				}

				if (typeof(ITask).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
				{
					Container.RegisterType(typeof(ITask), t, t.Name);
				}
			}
		}
	}
}