using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Container;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.DPC.Loader.Container;

namespace QA.Core.ProductCatalog.Actions.Tests.IntegrationTests
{
	public abstract class IntegrationTestsBase
	{
		protected IUnityContainer Container { get; private set; }
		protected Func<string, IAction> ActionFactory { get; private set; }

		[TestInitialize]
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