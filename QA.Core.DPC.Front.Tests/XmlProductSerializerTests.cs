using QA.Core.DPC.Front;
using Xunit;

namespace QA.Core.DPC.Integration.Tests
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
