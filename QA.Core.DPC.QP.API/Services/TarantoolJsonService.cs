using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.Infrastructure;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Options;
using QA.Core.DPC.QP.Models;
using QA.ProductCatalog.ContentProviders;

namespace QA.Core.DPC.QP.API.Services
{
    public class TarantoolJsonService : IProductSimpleService<JToken, JToken>
    {
        private readonly TimeSpan _expiration;
        private readonly Uri _baseUri;

        private readonly ISettingsService _settingsService;
        private readonly IVersionedCacheProvider _cacheProvider;
        private readonly IHttpClientFactory _factory;

        public TarantoolJsonService(ISettingsService settingsService, IVersionedCacheProvider cacheProvider, 
            IHttpClientFactory factory, IOptions<IntegrationProperties> intProps)
        {
            var tntUrl = intProps.Value.TarantoolApiUrl;
            _baseUri = !String.IsNullOrEmpty(tntUrl) ? new Uri(tntUrl) : null;
            _settingsService = settingsService;
            _cacheProvider = cacheProvider;
            _expiration = TimeSpan.FromHours(1);
    }

        public JToken GetProduct(string customerCode, int productId, int definitionId, bool isLive = false)
        {
            var productUrl = GetProductUrl(customerCode, productId, definitionId, isLive);
            var uri = new Uri(_baseUri, productUrl);
            return Get<JObject>(uri);
        }

        public JToken GetDefinition(string customerCode, int definitionId)
        {
            DefinitionDescriptor definition = null;
            var key = $"tnt_definition_{customerCode}_{definitionId}";            

            if (_cacheProvider.TryGetValue(key, out object value))
            {
                var definitionCache = value as DefinitionDescriptor;
                var modifiedActual = GetDefinitionModifiedDate(customerCode, definitionId);

                if (modifiedActual == definitionCache.Modified)
                {
                    definition = definitionCache;
                }
            }

            if (definition == null)
            {
                var actualDefinition = GetActualDefinition(customerCode, definitionId);

                var t = actualDefinition.SelectTokens("result[?(@.CONTENT_ITEM_ID)]").Select(n => (JObject)n).First();
                var last = t.Children().Last(n => ((JProperty)n).Name.StartsWith("field_")) as JProperty;

                if (last == null)
                {
                    throw new InvalidOperationException("Cannot find definition data");
                }

                definition = new DefinitionDescriptor
                {
                    Modified = actualDefinition["result"].First().Value<DateTime>("MODIFIED"),
                    JsonDefinition = JObject.Parse(last.Value.ToString())
                };

                _cacheProvider.Add(definition, key, new string[0], _expiration);
            }

            return definition.JsonDefinition;
        }

        private JToken GetActualDefinition(string customerCode, int definitionId)
        {
            var definitionUrl = GetDefinitiontUrl(customerCode, definitionId, false);
            var uri = new Uri(_baseUri, definitionUrl);
            var result = Get<JObject>(uri);
            return result;
        }

        private DateTime GetDefinitionModifiedDate(string customerCode, int definitionId)
        {
            var definitionUrl = GetDefinitiontUrl(customerCode, definitionId, true);
            var uri = new Uri(_baseUri, definitionUrl);
            var result = Get<JObject>(uri);
            return result["result"].First().Value<DateTime>("MODIFIED");
        }        

        private string GetDefinitiontUrl(string customerCode, int definitionId, bool onlySystem)
        {
            var settingId = int.Parse(_settingsService.GetSetting(SettingsTitles.PRODUCT_DEFINITIONS_CONTENT_ID));
            return $"{customerCode}/product-building/data/articles?invariant-name=content_{settingId}&take=1&skip=0&key={definitionId}&only_system={onlySystem.ToString().ToLower()}";
        }

        private string GetProductUrl(string customerCode, int productId, int definitionId, bool isLive)
        {
            return $"{customerCode}/product-building/?product_id={productId}&definition_id={definitionId}&is_united={(!isLive).ToString().ToLower()}&include_sys_fields=true";
        }

        private T Get<T>(Uri uri)
        {
            var client = _factory.CreateClient();
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

    internal class DefinitionDescriptor
    {
        public DateTime Modified { get; set; }
        public JToken JsonDefinition { get; set; }
    }
}