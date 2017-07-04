using Xunit;

namespace QA.Core.DPC.Front.Tests
{
	public class XmlProductSerializerTests : TestBase
	{
		[Fact]
		public void Deserialize_Xml()
		{
			DeserializeData(new XmlProductSerializer(), "product.xml");	
		}
	}
}
