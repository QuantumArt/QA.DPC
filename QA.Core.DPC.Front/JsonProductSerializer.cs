using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QA.Core.DPC.Front
{
	public class JsonProductSerializer : IProductSerializer
	{
		public ProductInfo Deserialize(string data)
		{
			string productData = JObject.Parse(data)["product"].ToString();
			var product = JsonConvert.DeserializeObject<Product>(productData);

			return new ProductInfo
			{
				Products = new[] { product }
			};
		}
	}
}
