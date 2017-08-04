using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.Infrastructure;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace QA.Core.DPC.QP.API.Services
{
    public class TarantoolJsonService : IProductSimpleService<JToken, JToken>
    {
        private readonly Uri _baseUri;

        private readonly ISettingsService _settingsService;
        
        public TarantoolJsonService(ISettingsService settingsService)
        {
            _baseUri = new Uri(ConfigurationManager.AppSettings["DPC.Tarantool.Api"]);
            _settingsService = settingsService;
        }

        public JToken GetDefinition(string customerCode, int definitionId)
        {
            var definitionUrl = GetDefinitiontUrl(customerCode, definitionId);
            var uri = new Uri(_baseUri, definitionUrl);
            var t = Get<JObject>(uri).SelectTokens("result[?(@.CONTENT_ITEM_ID)]").Select(n => (JObject)n).First();
            return t.Children().Last(n => ((JProperty)n).Name.StartsWith("field_"));
        }

        public JToken GetProduct(string customerCode, int productId, int definitionId, bool isLive = false)
        {
            var productUrl = GetProductUrl(customerCode, productId, definitionId, isLive);
            var uri = new Uri(_baseUri, productUrl);
            return Get<JObject>(uri);
        }

        private string GetDefinitiontUrl(string customerCode, int definitionId)
        {
            var settingId = int.Parse(_settingsService.GetSetting(SettingsTitles.PRODUCT_DEFINITIONS_CONTENT_ID));
            return $"{customerCode}/product-building/data/articles?invariant-name=content_{settingId}&take=1&skip=0&key={definitionId}";
        }

        private string GetProductUrl(string customerCode, int productId, int definitionId, bool isLive)
        {
            return $"{customerCode}/product-building/?product_id={productId}&definition_id={definitionId}&s_united={!isLive}";
        }

        private T Get<T>(Uri uri)
        {
            using (var client = new HttpClient())
            using (var response = client.GetAsync(uri).Result)
            {
                if (!response.IsSuccessStatusCode) throw new Exception($"unsuccess request: {uri}");
                var stream = response.Content.ReadAsStreamAsync().Result;
                return Deserialize<T>(stream);
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