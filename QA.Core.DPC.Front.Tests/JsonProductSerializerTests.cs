using Xunit;

namespace QA.Core.DPC.Front.Tests
{
	public class JsonProductSerializerTests : TestBase
	{
		[Fact]
		public void Deserialize_Json()
		{
			DeserializeData(new JsonProductSerializer(), "product.json");	
		}
	}
}
