using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Core.DPC.QP.Autopublish.Models;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;

namespace QA.Core.DPC.QP.Autopublish.Services
{
    public class AutopublishProvider : IAutopublishProvider
    {
        private const string DeleteMethod = "DELETE";
        private const string GetMethod = "GET";

        private readonly Uri _baseUri;

        public AutopublishProvider()
        {
            _baseUri = new Uri(ConfigurationManager.AppSettings["Autopublish.SyncApi"]);
        }

        public ProductItem[] Peek(string customerCode)
        {
            var url = GetPeekUrl(customerCode);
            var result = Request(url, GetMethod);

            ValidateStatus(result);

            return result["data"]
                  .Select(itm => new ProductItem
                  {
                      CustomerCode = customerCode,
                      ProductId = itm["id"].Value<int>(),
                      DefinitionId = itm["definitionId"].Value<int>(),
                      Slug = itm["meta"]["slug"].Value<string>(),
                      Version = itm["meta"]["version"].Value<string>(),
                  })
                  .ToArray();
        }

        public ProductDescriptor GetProduct(ProductItem item)
        {
            var url = GetProductUrl(item);
            var result = Request(url, GetMethod);

            ValidateStatus(result);

            return new ProductDescriptor
            {
                CustomerCode = item.CustomerCode,
                ProductId = item.ProductId,
                DefinitionId = item.DefinitionId,
                Slug = item.Slug,
                Version = item.Version,
                Product = result["data"]["item"]["product"].ToString(),
                Definition = result["data"]["meta"]["definition"].ToString()
            };
        }

        public void Dequeue(ProductItem item)
        {
            var url = GetDequeueUrl(item);
            var result = Request(url, DeleteMethod);

            ValidateStatus(result);
        }

        private JObject Request(string url, string method, JObject data = null)
        {
            var uri = new Uri(_baseUri, url);
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = method;
            request.Accept = "application/json";

            var serializer = new JsonSerializer();

            if (data != null)
            {
                using (var stream = request.GetRequestStream())
                using (var writer = new StreamWriter(stream))
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    serializer.Serialize(jsonWriter, data);
                    jsonWriter.Flush();
                }
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(reader))
            {
                return serializer.Deserialize<JObject>(jsonReader);
            }
        }

        private string GetPeekUrl(string customerCode)
        {
            return $"{customerCode}/queue";
        }

        private string GetProductUrl(ProductItem item)
        {
            return $"{item.CustomerCode}/product/{item.CustomerCode}/?product_id={item.ProductId}&definition_id={item.DefinitionId}";
        }

        private string GetDequeueUrl(ProductItem item)
        {
            return $"{item.CustomerCode}/queue/?product_id={item.ProductId}&definition_id={item.DefinitionId}";
        }

        private void ValidateStatus(JObject item)
        {
            var status = item["status"].Value<string>();

            if (status != "success")          
            {
                throw new Exception($"incorrect status {status}");
            }
        }
    }
}
