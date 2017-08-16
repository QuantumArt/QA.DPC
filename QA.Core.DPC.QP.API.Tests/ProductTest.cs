using NUnit.Framework;
using QA.Core.DPC.QP.API.Tests.Attributes;
using QA.Core.DPC.QP.API.Tests.Providers;
using QA.Core.Models.Entities;
using System.Linq;

namespace QA.Core.DPC.QP.API.Tests
{
    [TestFixture]
    public class ProductTest
    {
        [Test, Sequential]
        public void Product_Articles_Match(
            [Parameter("db_product")] string dbProductName,
            [Parameter("tnt_product")] string tntProductName,
            [Parameter("tnt_definition")] string tntDefinitionName)
        {
            var provider = new ProductProvider(dbProductName, tntProductName, tntDefinitionName);

            #if DEBUG
            provider.SaveProducts();
            #endif

            TestContext.WriteLine("db articles: {0}", provider.DbOnlyKeys.Length);
            TestContext.WriteLine("tnt articles: {0}", provider.TntOnlyKeys.Length);
            TestContext.WriteLine("joint articles: {0}", provider.JointKeys.Length);
            
            Assert.Multiple(() =>
            {
                Assert.That(provider.DbOnlyKeys, Is.Empty, "articles only in db");
                Assert.That(provider.TntOnlyKeys, Is.Empty, "articles only in tnt");              
            });            
        }

        [Test, Sequential]
        public void Product_Articles_Compare(
            [Parameter("db_product")] string dbProductName,
            [Parameter("tnt_product")] string tntProductName,
            [Parameter("tnt_definition")] string tntDefinitionName)
        {
            var provider = new ProductProvider(dbProductName, tntProductName, tntDefinitionName);

            foreach (var key in provider.JointKeys)
            {
                var dbArticle = provider.DbDictionary[key];
                var tntArticle = provider.TntDictionary[key];

                TestContext.WriteLine(key);
                CompareArticle(dbArticle, tntArticle, key);                
            }
        }

        [Test, Sequential]
        public void Product_Fields_Match(
            [Parameter("db_product")] string dbProductName,
            [Parameter("tnt_product")] string tntProductName,
            [Parameter("tnt_definition")] string tntDefinitionName)
        {
            var provider = new ProductProvider(dbProductName, tntProductName, tntDefinitionName);

            foreach (var key in provider.JointKeys)
            {
                var dbArticle = provider.DbDictionary[key];
                var tntArticle = provider.TntDictionary[key];

                var dbNames = dbArticle.Fields.Values.OfType<PlainArticleField>().Select(f => f.FieldName).ToArray();
                var tntNames = tntArticle.Fields.Values.OfType<PlainArticleField>().Select(f => f.FieldName).ToArray();

                var dbOnlyNames = dbNames.Except(tntNames).ToArray();
                var tntOnlyNames = tntNames.Except(dbNames).ToArray();

                Assert.Multiple(() =>
                {
                    Assert.That(dbOnlyNames, Is.Empty, $"{key} fields only in db");
                    Assert.That(tntOnlyNames, Is.Empty, $"{key} fields only in tnt");
                });               
            }
        }

        [Test, Sequential]
        public void Product_Fields_Compare(
            [Parameter("db_product")] string dbProductName,
            [Parameter("tnt_product")] string tntProductName,
            [Parameter("tnt_definition")] string tntDefinitionName)
        {
            var provider = new ProductProvider(dbProductName, tntProductName, tntDefinitionName);

            foreach (var key in provider.JointKeys)
            {
                var dbArticle = provider.DbDictionary[key];
                var tntArticle = provider.TntDictionary[key];

                var dbNames = dbArticle.Fields.Values.OfType<PlainArticleField>().Select(f => f.FieldName).ToArray();
                var tntNames = tntArticle.Fields.Values.OfType<PlainArticleField>().Select(f => f.FieldName).ToArray();
                var jointNames = dbNames.Intersect(tntNames).ToArray();

                foreach(var name in jointNames)
                {
                    var dbField = (PlainArticleField)dbArticle.Fields[name];
                    var tntField = (PlainArticleField)tntArticle.Fields[name];

                    ComparePlainArticleField(dbField, tntField, key);
                }
            }
        }

        private void CompareArticle(Article dbArticle, Article tntArticle, string path)
        {
            Assert.Multiple(() =>
            {
                Assert.That(dbArticle.ContentId, Is.EqualTo(tntArticle.ContentId), $"{path}_ContentId");
                Assert.That(dbArticle.ContentName, Is.EqualTo(tntArticle.ContentName), $"{path}_ContentName");
                Assert.That(dbArticle.ContentDisplayName, Is.EqualTo(tntArticle.ContentDisplayName), $"{path}_ContentDisplayName");

                Assert.That(dbArticle.Visible, Is.EqualTo(tntArticle.Visible), $"{path}_Visible");
                Assert.That(dbArticle.Archived, Is.EqualTo(tntArticle.Archived), $"{path}_Archived");
                Assert.That(dbArticle.Created, Is.EqualTo(tntArticle.Created), $"{path}_Created");
                Assert.That(dbArticle.Modified, Is.EqualTo(tntArticle.Modified), $"{path}_Modified");
                Assert.That(dbArticle.IsPublished, Is.EqualTo(tntArticle.IsPublished), $"{path}_IsPublished");
                Assert.That(dbArticle.Status, Is.EqualTo(tntArticle.Status), $"{path}_Status");
                Assert.That(dbArticle.Splitted, Is.EqualTo(tntArticle.Splitted), $"{path}_Splitted");
                Assert.That(dbArticle.PublishingMode, Is.EqualTo(tntArticle.PublishingMode), $"{path}_PublishingMode");
            });
        }

        private void CompareArticleField(ArticleField dbField, ArticleField tntField, string path)
        {
            Assert.That(dbField.ContentId, Is.EqualTo(tntField.ContentId), $"{path}_ContentId");
            Assert.That(dbField.FieldId, Is.EqualTo(tntField.FieldId), $"{path}_FieldId");
            Assert.That(dbField.FieldName, Is.EqualTo(tntField.FieldName), $"{path}_FieldName");
            Assert.That(dbField.FieldDisplayName, Is.EqualTo(tntField.FieldDisplayName), $"{path}_FieldDisplayName");
        }

        private void ComparePlainArticleField(PlainArticleField dbField, PlainArticleField tntField, string path)
        {
            Assert.Multiple(() =>
            {
                CompareArticleField(dbField, tntField, path);

                Assert.That(dbField.Value, Is.EqualTo(tntField.Value), $"{path}_Value");
                Assert.That(dbField.NativeValue, Is.EqualTo(tntField.NativeValue), $"{path}_NativeValue");
                Assert.That(dbField.PlainFieldType, Is.EqualTo(tntField.PlainFieldType), $"{path}_PlainFieldType");
            });
        }   
    }
}
