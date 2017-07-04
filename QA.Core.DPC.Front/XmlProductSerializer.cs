using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace QA.Core.DPC.Front
{
	public class XmlProductSerializer : IProductSerializer
	{
		public ProductInfo Deserialize(string data)
		{
			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(data.Replace(" xsi:type", " productType"))))
			{
				var ser = new XmlSerializer(typeof(ProductInfo));
				return (ProductInfo)ser.Deserialize(ms);
			}
		}
	}
}
