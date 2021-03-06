﻿using System.Linq;
using System.Reflection;
using System.Xaml;
using NUnit.Framework;
using Portable.Xaml;
using QA.Core.DPC.UI;
using QA.Core.Models.Configuration;
using QA.Core.Models.Tests.Controls;

namespace QA.Core.Models.Tests
{
    [TestFixture]
    public class CustomSerializingTests
    {
        [Test]
        public void Test_XamlServices()
        {
            var t = Get<QPControlTest>("QA.Core.Models.Tests.Xaml.Test001.xaml");
        }

        [Test]
        public void Test_XamlServices_string_content()
        {
            var t = Get<QPControlTest>("QA.Core.Models.Tests.Xaml.Test001.xaml");
            var tested = (((StackPanel)((QPControlTest)t.Content).Content).Items[1]) as QPControlTest;

            Assert.IsNotNull(tested);
            Assert.IsInstanceOf<string>(tested.Content);
            Assert.AreEqual("static text", tested.Content);
        }
        
        [Test]
		public void Test_XamlServices_recursion()
		{
			var t = Get<Content>("QA.Core.Models.Tests.Xaml.Recursive.xaml");

			Assert.IsNotNull(t);
		}

		[Test]
		public void Test_XamlServices_Equality()
		{
			var t = Get<Content>("QA.Core.Models.Tests.Xaml.Recursive.xaml");

			var content = ((EntityField) t.Fields.First()).Content;

			var sameReferenceContent = ((EntityField) content.Fields.First()).Content;

			var copyContent = ((EntityField)t.Fields.ElementAt(1)).Content;

			Assert.IsTrue(content.Equals(sameReferenceContent));

			Assert.IsTrue(content.Equals(copyContent));
		}

        private static T Get<T>(string path)
        {
            using (var stream = Assembly.GetExecutingAssembly()
              .GetManifestResourceStream(path))
            {
                // создаем экземпляр валидатора

                XamlXmlReader xxr = new XamlXmlReader(stream);
                //where xamlStringToLoad is a string of well formed XAML
                XamlObjectWriter xow = new XamlObjectWriter(xxr.SchemaContext);
                while (xxr.Read())
                {
                    xow.WriteNode(xxr);
                }


                return (T)xow.Result;
            }
        }
    }
}
