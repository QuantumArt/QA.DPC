using Microsoft.Practices.Unity;
using QA.ProductCatalog.Infrastructure;
using System;

namespace QA.ProductCatalog.Validation.Configuration
{
	public class ValidationConfiguration : UnityContainerExtension
	{
		protected override void Initialize()
		{
			Container.RegisterType<Func<string, IRemoteValidator>>(new InjectionFactory(c => new Func<string, IRemoteValidator>(key => c.Resolve<IRemoteValidator>(key))));
			Container.RegisterRemoteValidators<ValidationConfiguration>();
		}
	}
}
