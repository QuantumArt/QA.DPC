using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.DPC.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.DPC.Integration.Tests
{
	[TestClass]
	public class XmlProductSerializerTests : TestBase
	{
		[TestMethod]
		public void Deserialize_Xml()
		{
			DeserializeData(new XmlProductSerializer(), "product.xml");	
		}
	}
}
