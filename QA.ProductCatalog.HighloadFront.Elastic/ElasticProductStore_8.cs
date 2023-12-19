using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Json.More;
using Microsoft.Extensions.Logging;
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

        
        public override JsonObject GetDefaultIndexSettings()
        {
            var indexSettings = new JsonObject()
            {
                ["settings"] = new JsonObject()
                {
                    ["max_result_window"] = Options.MaxResultWindow,
                    ["mapping.total_fields.limit"] = Options.TotalFieldsLimit,
                    ["analysis"] = GetAnalysis()
                },
                ["mappings"] = GetMapping(string.Empty, Options.NotAnalyzedFields)
            };

            return indexSettings;
        }

        protected override JsonObject GetMapping(string type, string[] fields)
        {
            var formats = new JsonArray(GetDynamicDateFormatsFromConfig("Elastic8").Select(n => JsonValue.Create(n)).ToArray());
            var templates = new JsonArray(fields.Select(n => GetKeywordTemplate(type, n)).ToArray());
            templates = AddEdgeNgramTemplates(templates, type);
            templates.Add(GetTextTemplate());

            return new JsonObject()
            {
                ["dynamic_date_formats"] = formats,
                ["dynamic_templates"] = templates
            };
        }

        public override async Task<string> SearchAsync(ProductsOptionsBase options, string language, string state, CancellationToken cancellationToken = default)
        {
            var q = GetQuery(options).ToString();
            var client = Configuration.GetElasticClient(language, state);
            return await client.SearchAsync(string.Empty, q, cancellationToken);
        }

        public override async Task<string> FindByIdAsync(ProductsOptionsBase options, string language, string state)
        {
            var client = Configuration.GetElasticClient(language, state);
            return await client.FindSourceByIdAsync($"_source/{options.Id}", string.Empty, "_source_includes", options?.PropertiesFilter?.ToArray());
        }

        public override async Task<SonicResult> UpdateAsync(JsonElement product, string language, string state)
        {
            var id = GetId(product);
            if (id == null)
            {
                return SonicResult.Failed(SonicErrorDescriber
                    .StoreFailure("Product has no id"));
            }

            var client = Configuration.GetElasticClient(language, state);
            try
            {
                await client.UpdateAsync($"{id}/_update", string.Empty, product.ToString());
                return SonicResult.Success;
            }
            catch (Exception e)
            {
                return SonicResult.Failed(SonicErrorDescriber.StoreFailure(e.Message, e));
            }
        }
    }
}
