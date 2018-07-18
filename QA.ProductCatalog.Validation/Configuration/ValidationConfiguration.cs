using QA.ProductCatalog.Infrastructure;
using System;
using Unity;
using Unity.Extension;
using Unity.Injection;

namespace QA.ProductCatalog.Validation.Configuration
{
    public class ValidationConfiguration : UnityContainerExtension
	{
		protected override void Initialize()
		{
			Container.RegisterType<Func<string, IRemoteValidator2>>(new InjectionFactory(c => new Func<string, IRemoteValidator2>(key => c.Resolve<IRemoteValidator2>(key))));
			Container.RegisterRemoteValidators<ValidationConfiguration>();
		}
	}
}
