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

        private readonly Uri _baseTntUri;
        private readonly Uri _baseWebApiUri;

        public AutopublishProvider()
        {
            _baseTntUri = new Uri(ConfigurationManager.AppSettings["DPC.Tarantool.Api"]);
            _baseWebApiUri = new Uri(ConfigurationManager.AppSettings["DPC.WebApi"]);
        }

        public ProductItem[] Peek(string customerCode)
        {
            var url = GetPeekUrl(customerCode);
            var result = RequestQueue(url, GetMethod);

            ValidateStatus(result);

            return result["data"]
                  .Select(itm => new ProductItem
                  {
                      CustomerCode = customerCode,
                      ProductId = itm.Value<int>("product_id"),
                      DefinitionId = itm.Value<int>("definition_id"),
                      IsUnited = itm.Value<bool>("is_united"),
                      Action =  itm.Value<string>("action")
                  })
                  .ToArray();
        }

        public ProductDescriptor GetProduct(ProductItem item, string format)
        {
            var url = GetProductUrl(item, format);
            var result = RequestProduct(url);

            return new ProductDescriptor
            {
                CustomerCode = item.CustomerCode,
                ProductId = item.ProductId,
                DefinitionId = item.DefinitionId,
                Product = result
            };
        }

        public void Dequeue(ProductItem item)
        {
            var url = GetDequeueUrl(item);
            var result = RequestQueue(url, DeleteMethod);

            ValidateStatus(result);
        }

        private string RequestProduct(string url)
        {
            var uri = new Uri(_baseWebApiUri, url);
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = GetMethod;
            request.Accept = "application/json";

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return reader.ReadToEnd();
                }
                else
                {
                    throw new Exception($"Incorrect request  {uri}");
                }
            }
        }

        private JObject RequestQueue(string url, string method)
        {
            var uri = new Uri(_baseTntUri, url);
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = method;
            request.Accept = "application/json";

            var serializer = new JsonSerializer();         

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
            return $"{customerCode}/product-building/autopub";
        }

        private string GetProductUrl(ProductItem item, string format)
        {
            var absent = item.Action.ToLower() != "upserted";
            return $"api/{item.CustomerCode}/tarantool/{format}/{item.ProductId}?definitionId={item.DefinitionId}&absent={absent}&isLive={!item.IsUnited}&includeRegionTags=false";
        }

        private string GetDequeueUrl(ProductItem item)
        {
            return $"{item.CustomerCode}/product-building/autopub/?product_id={item.ProductId}&definition_id={item.DefinitionId}&is_united={item.IsUnited}";
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
