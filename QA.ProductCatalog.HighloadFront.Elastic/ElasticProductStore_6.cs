using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        protected override JObject GetDefaultIndexSettings()
        {
            var indexSettings = new JObject(
                new JProperty("settings", new JObject(
                    new JProperty("max_result_window", Options.MaxResultWindow),
                    new JProperty("mapping.total_fields.limit", Options.TotalFieldsLimit)
                )),
                new JProperty("mappings", GetMappings(new[] { "_doc" }, Options.NotAnalyzedFields))
            );
            return indexSettings;
        }

        protected override void SetQuery(JObject json, ProductsOptions productsOptions)
        {
            JProperty query = null;
            var shouldGroups = new List<List<JProperty>>();
            var currentGroup = new List<JProperty>();
            JProperty typeFilter = null;

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
                    if (condition == null)
                        continue;

                    if (condition.Value["or"] != null)
                    {
                        if (currentGroup.Any())
                        {
                            shouldGroups.Add(currentGroup);
                        }
                        condition.Value["or"].Parent.Remove();
                        currentGroup = new List<JProperty>();
                    }

                    currentGroup.Add(condition);
                }
                shouldGroups.Add(currentGroup);
            }

            if (currentGroup.Any() || shouldGroups.Any())
            {
                query = shouldGroups.Count <= 1 ? Must(currentGroup) : Should(shouldGroups.Select(Must));
                json.Add(new JProperty("query", new JObject(query)));
            }
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
