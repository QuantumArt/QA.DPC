using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.ProductCatalog.Actions.Tests.UnitTests
{
	public abstract class ExceptionTestsBase<T>
		where T : Exception
	{
		[TestMethod]
		public void ExceptionSerialization()
		{
			T originalException = GetException();
			T restoredRestored = null;

			using (Stream s = new MemoryStream())
			{
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(s, originalException);
				s.Position = 0;
				restoredRestored = (T)formatter.Deserialize(s);
			}

			Assert.IsNotNull(restoredRestored);
			ValidateException(originalException, restoredRestored);
		}

		abstract protected T GetException();
		abstract protected void ValidateException(T originalException, T restoredRestored);
	}
}
