using System;
using System.Linq;
//using Microsoft.Practices.Unity;
using NUnit.Framework;
using QA.ProductCatalog.Infrastructure;
using QA.Core.DPC.API.Test.Fakes;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.API.Container;

namespace QA.Core.DPC.API.Test.Tests
{
	//[TestFixture]
	//public class ProductUpdateServiceTest : TestBase
	//{
	//	protected ProductServiceFake ProductService { get; private set; }
	//	protected ArticleServiceFake ArticleService { get; private set; }

	//	protected override void Initialize(IUnityContainer container)
	//	{
	//		ProductService = new ProductServiceFake();
	//		ArticleService = new ArticleServiceFake();

	//		Container.AddNewExtension<APIContainerConfiguration>();
	//		container.RegisterInstance<IProductService>(ProductService);
	//		container.RegisterInstance<IArticleService>(ArticleService);
	//	}

	//	#region Tests
	//	[Test]
	//	public void ProductUpdateService_NoUpdate()
	//	{
	//		var service = Container.Resolve<IProductUpdateService>();
	//		var originalProduct = GetProduct("original");
	//		ProductService.Product = originalProduct;
	//		service.Update(originalProduct, null);
	//		Assert.IsFalse(ArticleService.Articles.Any());
	//	}

	//	[Test]
	//	public void ProductUpdateService_Insert()
	//	{
	//		var service = Container.Resolve<IProductUpdateService>();
	//		var updatedProduct = GetProduct("template");
	//		ProductService.Product = null;
	//		service.Update(updatedProduct, null);
	//		Assert.AreEqual(4, ArticleService.Articles.Count());
	//	}

	//	[Test]
	//	public void ProductUpdateService_Update()
	//	{
	//		var service = Container.Resolve<IProductUpdateService>();
	//		var originalProduct = GetProduct("original");
	//		var updatedProduct = GetProduct("updated");
	//		ProductService.Product = originalProduct;
	//		service.Update(updatedProduct, null);
	//		Assert.AreEqual(4, ArticleService.Articles.Count());
	//	}		
	//	#endregion
	//}
}
