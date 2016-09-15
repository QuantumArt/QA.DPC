using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QA.Core.ProductCatalog.Actions.Tests.UnitTests
{
    public abstract class ExceptionTestsBase<T>
        where T : Exception
    {
        [Ignore]
        [TestMethod]
        public void ExceptionSerialization()
        {
            var originalException = GetException();
            T restoredRestored = null;

            using (Stream s = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(s, originalException);
                s.Position = 0;
                restoredRestored = (T)formatter.Deserialize(s);
            }

            Assert.IsNotNull(restoredRestored);
            ValidateException(originalException, restoredRestored);
        }

        protected abstract T GetException();

        protected abstract void ValidateException(T originalException, T restoredRestored);
    }
}
