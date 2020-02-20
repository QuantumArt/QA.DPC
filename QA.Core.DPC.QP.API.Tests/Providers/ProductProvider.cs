using Newtonsoft.Json.Linq;
using NUnit.Framework;
using QA.Core.DPC.QP.API.Services;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using QA.Core.DPC.QP.Configuration;

namespace QA.Core.DPC.QP.API.Tests.Providers
{
    public class ProductProvider : ProviderBase
    {
        private string DbProductName { get; set; }
        private string TntProductName { get; set; }
        public Article DbProduct { get; private set; }
        public Article TntProduct { get; private set; }
        public Dictionary<string, Article> DbDictionary { get; private set; }
        public Dictionary<string, Article> TntDictionary { get; private set; }
        public string[] DbOnlyKeys { get; private set; }
        public string[] TntOnlyKeys { get; private set; }
        public string[] JointKeys { get; private set; }

        public ProductProvider(string dbProductName, string tntProductName, string tntDefinitionName)
        {
            DbProductName = dbProductName;
            TntProductName = tntProductName;
            DbProduct = GetDbProduct(dbProductName);
            TntProduct = GetTntProduct(tntProductName, tntDefinitionName);
            DbDictionary = GetArticleDictionary(DbProduct);
            TntDictionary = GetArticleDictionary(TntProduct);
            DbOnlyKeys = DbDictionary.Keys.Except(TntDictionary.Keys).ToArray();
            TntOnlyKeys = TntDictionary.Keys.Except(DbDictionary.Keys).ToArray();
            JointKeys = DbDictionary.Keys.Intersect(TntDictionary.Keys).ToArray();
        }

        public void SaveProducts()
        {
            var path = Path.Combine(Path.GetTempPath(), Assembly.GetCallingAssembly().GetName().Name);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var dbFileName = Path.Combine(path, DbProductName + ".xaml");
            var tntFileName = Path.Combine(path, TntProductName + ".xaml");
            SaveXaml(dbFileName, DbProduct);
            TestContext.WriteLine("db product saved to {0}", dbFileName);
            SaveXaml(tntFileName, TntProduct);            
            TestContext.WriteLine("tnt product saved to {0}", tntFileName);
        }

        private Article GetDbProduct(string productName)
        {
            return GetXaml<Article>($"QA.Core.DPC.QP.API.Tests.Data.{productName}.xaml");
        }

        private Article GetTntProduct(string productName, string definitionName)
        {
            var productJson = GetJson<JObject>($"QA.Core.DPC.QP.API.Tests.Data.{productName}.json");
            var definitionJson = GetJson<JObject>($"QA.Core.DPC.QP.API.Tests.Data.{definitionName}.json");
            var productService = GetProductService(productJson, definitionJson);
            return productService.GetProduct(0, 0);
        }

        private IProductSimpleAPIService GetProductService(JToken product, JToken definition)
        {
            var identityProvider = new IdentityProvider(new DefaultHttpContext()) { Identity = new Identity("customerCode") };
            var dataProvider = new CustomProductSimpleService<JToken, JToken>(product, definition);
            var statusProvider = new StatusProvider();
            var settingsService = new SettingsService();
            return new TarantoolProductAPIService(dataProvider, identityProvider, statusProvider, settingsService);
        }

        private Dictionary<string, Article> GetArticleDictionary(Article parent)
        {
            var dictionary = new Dictionary<string, Article>();
            FillArticleDictionary(dictionary, parent, parent.Id.ToString());
            return dictionary;
        }

        private void FillArticleDictionary(Dictionary<string, Article> dictionary, Article parent, string key)
        {
            Assert.That(dictionary, Is.Not.ContainKey(key));
            dictionary[key] = parent;

            foreach (var field in parent.Fields.Values)
            {
                if (field is IGetArticle singleArticleSource)
                {
                    var article = singleArticleSource.GetItem(null);

                    if (article != null)
                    {
                        Assert.That(field.FieldName, Is.Not.Null.And.Not.Empty);

                        string fieldKey;

                        if (field is ExtensionArticleField)
                        {
                            fieldKey = $"{key}_{field.FieldName}(ex)";
                        }
                        else
                        {
                            fieldKey = $"{key}_{field.FieldName}_{article.Id}";
                        }

                        FillArticleDictionary(dictionary, article, fieldKey);
                    }
                }
                else if (field is IEnumerable<Article> multiArticleSource)
                {
                    foreach (var article in multiArticleSource)
                    {
                        Assert.That(field.FieldName, Is.Not.Null.And.Not.Empty);
                        var fieldKey = $"{key}_{field.FieldName}_{article.Id}";
                        FillArticleDictionary(dictionary, article, fieldKey);
                    }
                }
            }
        }
    }
}
