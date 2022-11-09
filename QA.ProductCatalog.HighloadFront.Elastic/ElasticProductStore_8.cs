using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class ElasticProductStore_8 : ElasticProductStore_6
    {
        public ElasticProductStore_8(ElasticConfiguration config, SonicElasticStoreOptions options, ILoggerFactory loggerFactory) : base(config, options, loggerFactory)
        {
        }

        protected override string BuildRowMetadata(string name, string type, string id)
        {
            return $"{{\"index\":{{\"_index\":\"{name}\",\"_id\":\"{id}\"}}}}";
        }

        protected override JObject GetDefaultIndexSettings()
        {
            var indexSettings = new JObject(
                new JProperty("settings", new JObject(
                    new JProperty("max_result_window", Options.MaxResultWindow),
                    new JProperty("mapping.total_fields.limit", Options.TotalFieldsLimit)
                )),
                new JProperty("mappings", GetMapping(string.Empty, Options.NotAnalyzedFields))
            );
            return indexSettings;
        }

        protected override JObject GetMapping(string type, string[] fields)
        {
            var formats = new JArray(Options.DynamicDateFormats);
            var templates = new JArray(fields.Select(n => GetKeywordTemplate(type, n)));
            templates.Add(GetTextTemplate());

            return new JObject(
                new JProperty("dynamic_date_formats", formats),
                new JProperty("dynamic_templates", templates)
            );
        }
    }
}
