using NUnit.Framework;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Container;
using System;
using QA.Core.DPC.Loader.Container;
using Unity;

namespace QA.Core.ProductCatalog.Actions.Tests.IntegrationTests
{
    public abstract class IntegrationTestsBase
	{
		protected IUnityContainer Container { get; private set; }
		protected Func<string, IAction> ActionFactory { get; private set; }

		[SetUp]
		public void Initialize()
		{
			Container = new UnityContainer();
			Container.AddNewExtension<ProductLoaderContainerConfiguration>();
            Container.AddNewExtension<LoaderConfigurationExtension>();
			Container.AddNewExtension<ActionContainerConfiguration>();

			ActionFactory = Container.Resolve<Func<string, IAction>>();
		}
	}
}