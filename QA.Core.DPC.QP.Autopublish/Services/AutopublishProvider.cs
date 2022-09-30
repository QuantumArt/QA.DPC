using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Core.DPC.QP.Autopublish.Models;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using QA.Core.DPC.QP.Models;
using QA.ProductCatalog.ContentProviders;

namespace QA.Core.DPC.QP.Autopublish.Services
{
    public class AutopublishProvider : IAutopublishProvider
    {
        private const string DeleteMethod = "DELETE";
        private const string GetMethod = "GET";
        private const string PostMethod = "POST";

        private readonly ISettingsService _settingsService;
        private readonly Uri _baseTntUri;
        private readonly Uri _baseWebApiUri;
        private readonly IStatusProvider _statusProvider;
        private readonly IHttpClientFactory _factory;

        public AutopublishProvider(ISettingsService settingsService, IStatusProvider statusProvider, IOptions<IntegrationProperties> intProps, IHttpClientFactory factory)
        {
            _settingsService = settingsService;
            _statusProvider = statusProvider;
            _baseTntUri = string.IsNullOrEmpty(intProps.Value.TarantoolApiUrl) ? 
                new Uri(intProps.Value.TarantoolApiUrl) : null;
            _baseWebApiUri = string.IsNullOrEmpty(intProps.Value.DpcWebApiUrl)
                ? new Uri(intProps.Value.DpcWebApiUrl) : null;
            _factory = factory;
        }

        public ProductItem[] Peek(string customerCode)
        {
            var url = GetPeekUrl(customerCode);
            var result = RequestQueue(url, GetMethod);
            var typefield = _settingsService.GetSetting(SettingsTitles.PRODUCT_TYPES_FIELD_NAME);

            ValidateStatus(result);

            return result["data"]
                  .Select(itm => new ProductItem
                  {
                      CustomerCode = customerCode,
                      ProductId = itm.Value<int>("product_id"),
                      DefinitionId = itm.Value<int>("definition_id"),
                      IsUnited = itm.Value<bool>("is_united"),
                      Action =  itm.Value<string>("action"),
                      IsArchiveOld = itm["old_root_article"]?.Value<bool>("ARCHIVE"),
                      IsVisibleOld = itm["old_root_article"]?.Value<bool>("VISIBLE"),
                      TypeOld = itm["old_root_article"]?.Value<string>(typefield),
                      StatusOld = GetStatus(itm["old_root_article"]?.Value<int>("STATUS_TYPE_ID")),
                      IsArchiveNew = itm["new_root_article"]?.Value<bool>("ARCHIVE"),
                      IsVisibleNew = itm["new_root_article"]?.Value<bool>("VISIBLE"),
                      TypeNew = itm["new_root_article"]?.Value<string>(typefield),
                      StatusNew = GetStatus(itm["new_root_article"]?.Value<int>("STATUS_TYPE_ID"))
                  })
                  .ToArray();
        }

        public async void PublishProduct(ProductItem item)
        {
            var uri = new Uri(_baseWebApiUri, GetAutopublishUrl(item, true));
            var content = new StringContent(JsonConvert.SerializeObject(item), Encoding.UTF8, "application/json");
            var client = _factory.CreateClient();
            try
            {
                var result = await client.PostAsync(uri, content);
                result.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"{PostMethod} request on {uri} failed because of {ex.StatusCode}: {ex.Message}", ex);
            }
        }

        public void Dequeue(ProductItem item)
        {
            var url = GetDequeueUrl(item);
            var result = RequestQueue(url, DeleteMethod);

            ValidateStatus(result);
        }

        private string GetStatus(int? statusId)
        {
            if (statusId.HasValue)
            {
                return _statusProvider.GetStatusName(statusId.Value);
            }
            else
            {
                return null;
            }
        }

        private JObject RequestQueue(string url, string method)
        {
            var uri = new Uri(_baseTntUri, url);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            var request = new HttpRequestMessage(new HttpMethod(method), uri);
            try
            {
                var response = client.Send(request);
                response.EnsureSuccessStatusCode();
                return JsonConvert.DeserializeObject<JObject>(response.Content.ToString() ?? string.Empty);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"{method} request on {uri} failed with code {ex.StatusCode}: {ex.Message}", ex);
            }
        }

        private string GetPeekUrl(string customerCode)
        {
            return $"{customerCode}/product-building/autopub";
        }

        private string GetAutopublishUrl(ProductItem item, bool localize)
        {
            return $"api/{item.CustomerCode}/tarantool/publish/binary/{item.ProductId}?localize={localize}";
        }

        private string GetDequeueUrl(ProductItem item)
        {
            return $"{item.CustomerCode}/product-building/autopub/?product_id={item.ProductId}&definition_id={item.DefinitionId}&is_united={item.IsUnited.ToString().ToLower()}";
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
