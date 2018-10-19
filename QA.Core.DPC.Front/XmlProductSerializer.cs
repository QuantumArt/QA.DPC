using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace QA.Core.DPC.Front
{
	public class XmlProductSerializer : IProductSerializer
	{
		public static string PrepareXml(string text)   
		{   
			string re = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";   
			string result = Regex.Replace(text, re, "");
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
