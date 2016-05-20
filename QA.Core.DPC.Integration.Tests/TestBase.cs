using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.DPC.Integration.Tests
{
	[TestClass]
	public abstract class TestBase
	{
		private string GetEmbeddedResourceText(string path)
		{
			using (var stream = Assembly.GetExecutingAssembly()
			   .GetManifestResourceStream(path))
			{
				using (var textReader = new StreamReader(stream))
				{
					return textReader.ReadToEnd();
				}
			}
		}

		protected void DeserializeData(IProductSerializer serializer, string resource)
		{
			string data = GetEmbeddedResourceText("QA.Core.DPC.Integration.Tests.Data." + resource);
			ProductInfo info = serializer.Deserialize(data);

			Assert.IsNotNull(info);
			Assert.IsNotNull(info.Products);
			Assert.IsTrue(info.Products.Any());

			var product = info.Products[0];
			Assert.IsNotNull(product.MarketingProduct);
			Assert.AreEqual(1836443, product.MarketingProduct.Id);
			Assert.AreEqual("test", product.MarketingProduct.Alias);
			Assert.IsNotNull(product.MarketingProduct.Title);
			Assert.AreEqual("Tariff", product.ProductType);

			Assert.IsNotNull(product.Regions);
			Assert.IsTrue(product.Regions.Any());

			var region = product.Regions[0];
			Assert.AreEqual(19963, region.Id);
			Assert.AreEqual("chita", region.Alias);
			Assert.IsNotNull(region.Title);
		}
	}
}
