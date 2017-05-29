using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.Models.Configuration;
using System.IO;

namespace QA.Core.Models.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Test_ConfigurationSerializer_001()
        {
            Content entity = new Content
            {
                ContentId = 288,
                ContentName = "Продукты",
                LoadAllPlainFields = true,
            };

            var mp = new Content();

            mp.Fields.Add(new EntityField { FieldName = "Categories"});
            mp.Fields.Add(new EntityField { FieldName = "Family"});
            mp.Fields.Add(new EntityField { FieldName = "Tabs"});
            mp.Fields.Add(new EntityField { FieldName = "MarketingSign"});
            mp.Fields.Add(new EntityField { FieldName = "Modifiers"});

            entity.Fields.Add(new EntityField
            {
                FieldName = "MarketingProduct",
                FieldId = 1115,
                CloningMode = CloningMode.UseExisting,
                DeletingMode = DeletingMode.Keep,
                Content = mp
            });

            var n = new EntityField
            {
                FieldName = "Regions",
                FieldId = 1228,
                CloningMode = CloningMode.UseExisting,
                DeletingMode = DeletingMode.Keep,
                Content = new Content()
            };

            entity.Fields.Add(n);


            var t = new ExtensionField
            {
                FieldName = "Type"
            };

            t.ContentMapping.Add(305, new Content { LoadAllPlainFields = true });
            t.ContentMapping.Add(312, new Content { LoadAllPlainFields = true });

            entity.Fields.Add(t);


            entity.Fields.Add(new EntityField
            {
                FieldName = "Parameters",
                FieldId = 1193,
                CloningMode = CloningMode.Copy,
                DeletingMode = DeletingMode.Delete,
                Content = new Content()
            });



            // Attach behavior
            XmlMappingBehavior.SetBehavior(n, new XmlMappingBehavior
            {
                ExportField = "Title",
                ExportMode = ExportMode.ExportFieldOnly
            });

            var text = Tools.ConfigurationSerializer.GetXml(entity);

            Console.WriteLine(text);
        }

        [TestMethod]
        public void Test_ConfigurationSerializer_002()
        {
            TextReader tr = new StreamReader(@"Xaml\LoaderMapping.xaml");
            string xml = tr.ReadToEnd();

            //string xml = QA.Core.Models.Tests.Helpers.ValidationHelper.GetEmbeddedResourceText(@"LoaderMapping.xaml");

            Tools.ConfigurationSerializer.GetContent(xml);
        }
    }
}

