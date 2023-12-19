using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class ElasticProductStore_6 : ElasticProductStore
    {
        public ElasticProductStore_6(ElasticConfiguration config, SonicElasticStoreOptions options, ILoggerFactory loggerFactory, IProductInfoProvider productInfoProvider)
            : base(config, options, loggerFactory, productInfoProvider)
        {

        }

        public override string GetType(JsonElement product)
        {
            return "_doc";
        }

        public override async Task<string> SearchAsync(ProductsOptionsBase options, string language, string state, CancellationToken cancellationToken = default)
        {
            var q = GetQuery(options).ToString();
            var client = Configuration.GetElasticClient(language, state);
            return await client.SearchAsync("_doc", q, cancellationToken);
        }
        
        public override async Task<string> FindByIdAsync(ProductsOptionsBase options, string language, string state)
        {
            var client = Configuration.GetElasticClient(language, state);
            return await client.FindSourceByIdAsync($"{options.Id}/_source", "_all", "_source_includes", options?.PropertiesFilter?.ToArray());
        }

        protected override JsonObject GetMapping(string type, string[] fields)
        {
            var formats = new JsonArray(
                GetDynamicDateFormatsFromConfig("Default").Select(n => JsonValue.Create(n)).ToArray()
                );
            var templates = new JsonArray(
                fields.Select(n => GetKeywordTemplate(type, n)).ToArray()
                );
            templates = AddEdgeNgramTemplates(templates, type);
            templates.Add(GetTextTemplate());

            return new JsonObject()
            {
                ["dynamic_date_formats"] = formats,
                ["dynamic_templates"] = templates
            };
        }

        public override JsonObject GetDefaultIndexSettings()
        {
            var indexSettings = new JsonObject()
            {
                ["settings"] = new JsonObject()
                {
                    ["max_result_window"] = Options.MaxResultWindow,
                    ["mapping.total_fields.limit"] = Options.TotalFieldsLimit,
                    ["index"] = GetIndexAnalyzers()
                },
                ["mappings"] = GetMappings(new[] {"_doc"}, Options.NotAnalyzedFields)
            };

            return indexSettings;
        }

        protected override void SetQuery(JsonObject json, ProductsOptionsBase productsOptions)
        {
            KeyValuePair<string, JsonNode>? query;
            var shouldGroups = new List<List<KeyValuePair<string, JsonNode>?>>();
            var currentGroup = new List<KeyValuePair<string, JsonNode>?>();
            KeyValuePair<string, JsonNode>? typeFilter;

            var filters = productsOptions.Filters;

            if (productsOptions.ActualType != null)
            {
                typeFilter = GetSingleFilter(Options.TypePath, productsOptions.ActualType, ",", true);
                currentGroup.Add(typeFilter);
            }

            if (filters != null)
            {
                var conditions = filters.Select(n => CreateQueryElem(n, productsOptions.DisableOr, productsOptions.DisableNot, productsOptions.DisableLike));


                foreach (var condition in conditions)
                {
                    
                    if (condition.Value.Value["or"] != null)
                    {
                        if (currentGroup.Any())
                        {
                            shouldGroups.Add(currentGroup);
                        }
                        condition.Value.Value["or"].Parent.AsObject().Remove("or");
                        currentGroup = new List<KeyValuePair<string, JsonNode>?>();
                    }

                    currentGroup.Add(condition);
                }
                shouldGroups.Add(currentGroup);
            }

            if (currentGroup.Any() || shouldGroups.Any())
            {
                query = shouldGroups.Count <= 1 ? Must(currentGroup) : Should(shouldGroups.Select(Must));
                json.Add("query", new JsonObject() { query.Value });
            }
        }

        protected JsonObject GetTextTemplate()
        {
            return new JsonObject()
            {
                ["text"] = new JsonObject()
                {
                    ["match_mapping_type"] = "string",
                    ["mapping"] = new JsonObject()
                    {
                        ["type"] = "text",
                        ["fields"] = new JsonObject()
                        {
                            ["keyword"] = new JsonObject()
                            {
                                ["type"] = "keyword",
                                ["ignore_above"] = 256
                            }
                        }
                    }
                }
            };
        }
    }
}
