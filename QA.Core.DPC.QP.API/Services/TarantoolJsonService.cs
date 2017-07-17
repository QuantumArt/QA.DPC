using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.Infrastructure;
using System;
using System.Configuration;
using System.IO;
using System.Net.Http;

namespace QA.Core.DPC.QP.API.Services
{
    public class TarantoolJsonService : IProductSimpleService<JToken, JToken>
    {
        private readonly Uri _baseUri;
        private readonly Uri _tempBaseUri;

        public TarantoolJsonService()
        {
            _baseUri = new Uri(ConfigurationManager.AppSettings["DPC.Tarantool.Api"]);
            _tempBaseUri = new Uri("http://mscdev02:90/DPC.WebAPI/");
        }

        public JToken GetDefinition(string customerCode, int definitionId)
        {
            var definitionUrl = GetDefinitiontUrl(customerCode, definitionId);
            var uri = new Uri(_tempBaseUri, definitionUrl);
            return Get<JObject>(uri);
        }

        public JToken GetProduct(string customerCode, int productId, int definitionId, bool isLive = false)
        {
            var productUrl = GetProductUrl(customerCode, productId, definitionId, isLive);
            var uri = new Uri(_baseUri, productUrl);
            return Get<JObject>(uri);
        }

        private string GetDefinitiontUrl(string customerCode, int definitionId)
        {
            return $"api/{customerCode}/v1/Tariffs/schema/jsonDefinition";
        }

        private string GetProductUrl(string customerCode, int productId, int definitionId, bool isLive)
        {
            return $"product-building?product_id={productId}&product_def_name=definition_339_full&include_sys_fields=true";
        }

        private T Get<T>(Uri uri)
        {
            using (var client = new HttpClient())
            using (var response = client.GetAsync(uri).Result)
            {
                if (response.IsSuccessStatusCode)
                {
                    var stream = response.Content.ReadAsStreamAsync().Result;
                    return Deserialize<T>(stream);
                }
                else
                {
                    throw new Exception($"unsuccess request: {uri}");
                }
            }
        }

        private T Deserialize<T>(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            using (var jtr = new JsonTextReader(sr))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<T>(jtr);
            }
        }
    }
}