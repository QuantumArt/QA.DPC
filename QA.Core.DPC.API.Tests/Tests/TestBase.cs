using System.Net.Http;
using System.Xaml;
using Moq;
using NUnit.Framework;
using QA.Core.Logger;
using QA.Core.DPC.API.Container;
using QA.Core.Models.Entities;
using QA.Core.ProductCatalog;
using Unity;

namespace QA.Core.DPC.API.Test.Tests
{
	public class TestBase
	{
		protected static IUnityContainer Container { get; private set; }
		protected const string Slug = "cdpsubscriptions";
		protected const string MarketingSlug = "cdpmsubscriptions";
		protected const string Version = "v1";
		protected const int ProductId = 1740474;
		protected const int MarketingProductId = 1740340;

		#region Initialization
		[OneTimeSetUp]
		public void Initialize()
		{
			Container = new UnityContainer();
			Container.AddExtension(new Diagnostic());
			Container.RegisterConnectionString("");
			Container.AddNewExtension<ProxyAPIContainerConfiguration>();
			Container.RegisterType<ILogger, NullLogger>();
			Container.RegisterInstance(new Mock<IHttpClientFactory>().Object);
			Initialize(Container);
		}

		protected virtual void Initialize(IUnityContainer container)
		{
		}
		#endregion

		#region Protected methods
		protected Article GetProduct(int productId)
		{
			string path = "Content/Product_" + productId + ".xml";
			return (Article)XamlServices.Load(path);
		}
		protected Article GetProduct(string key)
		{
			string path = "Content/Product_" + key + ".xml";
			return (Article)XamlServices.Load(path);
		}
		#endregion
	}
}
