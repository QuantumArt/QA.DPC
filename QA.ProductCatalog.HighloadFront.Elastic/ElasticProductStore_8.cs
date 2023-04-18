using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class ElasticProductStore_8 : ElasticProductStore_6
    {
        public ElasticProductStore_8(ElasticConfiguration config, SonicElasticStoreOptions options, ILoggerFactory loggerFactory, IProductInfoProvider productInfoProvider)
            : base(config, options, loggerFactory, productInfoProvider)
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
                    new JProperty("mapping.total_fields.limit", Options.TotalFieldsLimit),
                    GetIndexAnalyzers().First
                )),
                new JProperty("mappings", GetMapping(string.Empty, Options.NotAnalyzedFields))
            );
            return indexSettings;
        }

        protected override JObject GetMapping(string type, string[] fields)
        {
            var formats = new JArray(GetDynamicDateFormatsFromConfig("Elastic8"));
            var templates = new JArray(fields.Select(n => GetKeywordTemplate(type, n)));
            templates = AddEdgeNgramTemplates(templates, type);
            templates.Add(GetTextTemplate());

            return new JObject(
                new JProperty("dynamic_date_formats", formats),
                new JProperty("dynamic_templates", templates)
            );
        }

        public override async Task<string> SearchAsync(ProductsOptions options, string language, string state, CancellationToken cancellationToken = default)
        {
            var q = GetQuery(options).ToString();
            var client = Configuration.GetElasticClient(language, state);
            return await client.SearchAsync(string.Empty, q, cancellationToken);
        }

        public override async Task<string> FindByIdAsync(ProductsOptions options, string language, string state)
        {
            var client = Configuration.GetElasticClient(language, state);
            return await client.FindSourceByIdAsync($"_source/{options.Id}", string.Empty, "_source_includes", options?.PropertiesFilter?.ToArray());
        }

        public override async Task<SonicResult> UpdateAsync(JObject product, string language, string state)
        {
            var id = GetId(product);
            if (id == null)
            {
                return SonicResult.Failed(SonicErrorDescriber
                    .StoreFailure("Product has no id"));
            }

            var json = JsonConvert.SerializeObject(product);
            var client = Configuration.GetElasticClient(language, state);

            try
            {
                await client.UpdateAsync($"{id}/_update", string.Empty, json);
                return SonicResult.Success;
            }
            catch (Exception e)
            {
                return SonicResult.Failed(SonicErrorDescriber.StoreFailure(e.Message, e));
            }
        }
    }
}
