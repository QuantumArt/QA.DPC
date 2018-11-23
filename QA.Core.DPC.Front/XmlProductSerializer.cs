using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace QA.Core.DPC.Front
{
	public class XmlProductSerializer : IProductSerializer
	{
		
		private static Regex _invalidXMLChars = new Regex(
			@"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
			RegexOptions.Compiled);
		
		public static string PrepareXml(string text)   
		{   
			string result = _invalidXMLChars.Replace(text, "");
			result = result.Replace(" xsi:type", " productType");
			return result;
		}   
		
		public ProductInfo Deserialize(string data)
		{
			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(PrepareXml(data))))
			{
				var ser = new XmlSerializer(typeof(ProductInfo));
				return (ProductInfo)ser.Deserialize(ms);
			}
		}
	}
}
