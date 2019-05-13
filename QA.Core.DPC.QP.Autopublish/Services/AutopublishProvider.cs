using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Core.DPC.QP.Autopublish.Models;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
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

        public AutopublishProvider(ISettingsService settingsService, IStatusProvider statusProvider)
        {
            _settingsService = settingsService;
            _statusProvider = statusProvider;
            _baseTntUri = new Uri(ConfigurationManager.AppSettings["DPC.Tarantool.Api"]);
            _baseWebApiUri = new Uri(ConfigurationManager.AppSettings["DPC.WebApi"]);
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

        public void PublishProduct(ProductItem item)
        {
            var url = GetAutopublishUrl(item, true);
            var uri = new Uri(_baseWebApiUri, url);
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = PostMethod;
            request.ContentType = "application/octet-stream";

            using (var stream = request.GetRequestStream())
            {
                new BinaryFormatter().Serialize(stream, item);
                stream.Flush();
            }

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
                    {
                        return;
                    }
                    else
                    {
                        throw new Exception($"{PostMethod} request on {uri} failed because of {response.StatusCode}: {response.StatusDescription}");
                    }
                }
            }
            catch (WebException ex)
            {
                Exception innerException = null;
                var formatter = new BinaryFormatter();
                var response = ex.Response as HttpWebResponse;

                using (var stream = ex.Response.GetResponseStream())
                {
                    innerException = formatter.Deserialize(stream) as Exception;
                }
              
                throw new Exception($"{PostMethod} request on {uri} failed because of {response.StatusCode} in {response.StatusDescription}", innerException);
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
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = method;
            request.Accept = "application/json";

            var serializer = new JsonSerializer();

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(reader))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return serializer.Deserialize<JObject>(jsonReader);
                }
                else
                {
                    throw new Exception($"{method} request on {uri} failed with code {response.StatusCode}: {response.StatusDescription}");
                }                
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
