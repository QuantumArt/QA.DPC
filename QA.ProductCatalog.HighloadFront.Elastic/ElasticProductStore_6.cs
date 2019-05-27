using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class ElasticProductStore_6 : ElasticProductStore
    {
        public ElasticProductStore_6(ElasticConfiguration config, SonicElasticStoreOptions options, ILoggerFactory loggerFactory)
            : base(config, options, loggerFactory)
        {

        }

        public override string GetType(JObject product)
        {
            return "_doc";
        }

        public override async Task<string> SearchAsync(ProductsOptions options, string language, string state)
        {
            var q = GetQuery(options).ToString();
            var client = Configuration.GetElasticClient(language, state);
            return await client.SearchAsync("_doc", q);
        }

        protected override JObject GetMapping(string type, string[] fields)
        {
            var formats = new JArray(Options.DynamicDateFormats);
            var templates = new JArray(fields.Select(n => GetKeywordTemplate(type, n)));
            templates.Add(GetTextTemplate());

            return new JObject(
                new JProperty(type, new JObject(
                    new JProperty("dynamic_date_formats", formats),
                    new JProperty("dynamic_templates", templates)
                ))
            );
        }

        protected JObject GetTextTemplate()
        {
            return new JObject(
                new JProperty($"text", new JObject(
                    new JProperty("match_mapping_type", "string"),
                    new JProperty("mapping", new JObject(
                        new JProperty("type", "text")
                    ))
                ))
            );
        }
    }
}
