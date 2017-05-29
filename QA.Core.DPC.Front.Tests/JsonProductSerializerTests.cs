using QA.Core.DPC.Front;
using Xunit;

namespace QA.Core.DPC.Integration.Tests
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
