using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Core.DPC.QP.Autopublish.Models;
using QA.ProductCatalog.Infrastructure;
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

        private readonly ISettingsService _settingsService;
        private readonly Uri _baseTntUri;
        private readonly Uri _baseWebApiUri;

        public AutopublishProvider(ISettingsService settingsService)
        {
            _settingsService = settingsService;
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
                      TypeOld = itm["old_root_article"]?.Value<string>(typefield),
                      IsArchiveNew = itm["new_root_article"]?.Value<bool>("ARCHIVE"),
                      TypeNew = itm["new_root_article"]?.Value<string>(typefield)
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
                Action = item.Action,
                IsArchiveOld = item.IsArchiveOld,
                IsArchiveNew = item.IsArchiveNew,
                IsUnited = item.IsUnited,
                TypeOld = item.TypeOld,
                TypeNew = item.TypeNew,
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
            var absent = item.PublishAction != PublishAction.Publish;
            var type = item.TypeNew ?? item.TypeOld;
            return $"api/{item.CustomerCode}/tarantool/{format}/{item.ProductId}?definitionId={item.DefinitionId}&absent={absent}&type={type}&isLive={!item.IsUnited}&includeRegionTags=false";
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
