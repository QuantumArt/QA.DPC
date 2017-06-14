using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace QA.Core.DPC.Front.Tests
{
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
			string data = GetEmbeddedResourceText(Assembly.GetExecutingAssembly().GetName().Name + ".Data." + resource);
			ProductInfo info = serializer.Deserialize(data);

			Assert.NotNull(info);
			Assert.NotNull(info.Products);
			Assert.True(info.Products.Any());

			var product = info.Products[0];
			Assert.NotNull(product.MarketingProduct);
			Assert.Equal(1836443, product.MarketingProduct.Id);
			Assert.Equal("test", product.MarketingProduct.Alias);
			Assert.NotNull(product.MarketingProduct.Title);
			Assert.Equal("Tariff", product.ProductType);

			Assert.NotNull(product.Regions);
			Assert.True(product.Regions.Any());

			var region = product.Regions[0];
			Assert.Equal(19963, region.Id);
			Assert.Equal("chita", region.Alias);
			Assert.NotNull(region.Title);
		}
	}
}
