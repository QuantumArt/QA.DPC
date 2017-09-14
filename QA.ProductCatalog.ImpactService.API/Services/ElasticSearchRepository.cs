using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Core;
using QA.Core.Logger;
using QA.ProductCatalog.HighloadFront.Elastic;

namespace QA.ProductCatalog.ImpactService.API.Services
{
    public class ElasticSearchRepository : ISearchRepository
    {
        private readonly string _sourceQuery = "hits.hits.[?(@._source)]._source";
        private readonly int _timeout = 15;
        private readonly ILogger _logger;

        private Regex GetRegionTagRegex(string tag)
        {
            return new Regex($@"[<\[]replacement[>\]]tag={tag}[<\[]/replacement[>\]]");
        }

        public ElasticSearchRepository(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<DateTimeOffset> GetLastUpdated(int[] productIds, SearchOptions options, DateTimeOffset defaultValue)
        {
            var result = await GetContent(GetJsonQuery(productIds, true), options);
            var dates = JObject.Parse(result).SelectTokens($"{_sourceQuery}.UpdateDate").Select(n => (DateTimeOffset)n).ToArray();
            return dates.Any() ? dates.Max() : defaultValue;
        }

        private string GetJsonQuery(int[] productIds, bool onlyModified = false)
        {
            var ids = string.Join(", ", productIds.Select(n => $@"""{n.ToString()}""").ToArray());
            var fieldsFilter = (onlyModified) ? @"_source: [""UpdateDate""]," : "";
            var query = $@"{{ {fieldsFilter} ""query"" : {{ ""ids"" : {{ ""values"" : [{ids}] }}}}}}";
            return query;
        }

        private string GetRegionQuery(string regionAlias)
        {
            return $@"{{ _source: [""Region.Id""], ""query"" : {{ ""term"" : {{ ""Region.Alias"" : ""{regionAlias}"" }}}}}}";
        }

        private string GetMrQuery(string[] regionAliases)
        {
            var regions = string.Join(", ", regionAliases.Select(n => $@"""{n.ToString()}""").ToArray());
            return $@"{{ _source: [""Region.Parent.Alias""], ""query"" : {{ ""terms"" : {{ ""Region.Alias"" : [{regions}] }}}}}}";
        }


        public async Task<JObject[]> GetProducts(int[] productIds, SearchOptions options)
        {
            var result = await GetContent(GetJsonQuery(productIds), options);

            result = await ProcessRegionTags(productIds, result, options);

            var hits = JObject.Parse(result).SelectTokens(_sourceQuery).ToArray();

            return hits.Select(n => (JObject)n).ToArray();
        }

        public async Task<bool> IsOneMacroRegion(string[] regions, SearchOptions options)
        {
            var newOptions = options.Clone();
            newOptions.TypeName = "RoamingRegion";
            var regionResult = await GetContent(GetMrQuery(regions), newOptions);
            var aliases = JObject.Parse(regionResult)
                .SelectTokens($"{_sourceQuery}.Region.Parent.Alias").Select(n => n.ToString()).ToArray();
            return aliases.Length > 1 && aliases.Distinct().Count() == 1;
        }

        private async Task<string> ProcessRegionTags(int[] productIds, string input, SearchOptions options)
        {
            var result = input;
            var tagsToProcess = await GetTagsToProcess(productIds, options);
            for (var i = 1; i <= 2; i++)
            {
                foreach (var tag in tagsToProcess)
                {
                    result = GetRegionTagRegex(tag.Key).Replace(result, tag.Value);
                }
            }
            return result;
        }

        private async Task<Dictionary<string, string>> GetTagsToProcess(int[] productIds, SearchOptions options)
        {
            var regionTagIds = productIds.Select(n => 0 - n).ToArray();
            var tagResult = await GetContent(GetJsonQuery(regionTagIds), options);
            var documents = JObject.Parse(tagResult).SelectTokens(_sourceQuery).ToArray();
            var tags = documents
                    .SelectMany(n => (JArray)n.SelectToken("RegionTags"))
                    .ToArray();

            var homeRegionId = await GetHomeRegionId(options);

            var tagsToProcess = new Dictionary<string, string>();

            foreach (var tag in tags)
            {
                var title = tag["Title"].ToString();

                if (tagsToProcess.ContainsKey(title)) continue;

                var value = GetTagValue(homeRegionId, tag);

                tagsToProcess.Add(title, value.Substring(1, value.Length - 2));
            }

            return tagsToProcess;
        }

        private async Task<int> GetHomeRegionId(SearchOptions options)
        {
            if (options.HomeRegion == null) return 0;

            var newOptions = options.Clone();
            newOptions.TypeName = "RoamingRegion";
            var regionResult = await GetContent(GetRegionQuery(newOptions.HomeRegion), newOptions);
            var homeRegionIdToken = JObject.Parse(regionResult)
                .SelectTokens(_sourceQuery)?.FirstOrDefault()?.SelectToken("Region.Id");
            return homeRegionIdToken != null ? (int) homeRegionIdToken : 0;
        }

        private static string GetTagValue(int homeRegionId, JToken tag)
        {
            string value = null;
            if (homeRegionId != 0)
            {
                var values = tag["Values"].SelectTokens("[?(@.Value)]");
                value = values.FirstOrDefault(n => ((JArray)n["RegionsId"]).Select(m => (int)m).Contains(homeRegionId))
                   ?.SelectToken("Value")?.ToString();
                     
            }
            value = JsonConvert.ToString(value ?? tag["Values"][0]["Value"].ToString());
            return value;
        }

        private IElasticClient GetElasticClient(SearchOptions options)
        {
            return QpElasticConfiguration.GetElasticClient(options.IndexName, options.BaseAddress, _logger, false, _timeout);
        }


        public async Task<string> GetContent(string json, SearchOptions options)
        {
            var client = GetElasticClient(options);

            ElasticsearchResponse<Stream> response; 
            if (options.TypeName == null)
            {
                response = await client.LowLevel.SearchAsync<Stream>(client.ConnectionSettings.DefaultIndex, json);
            }
            else
            {
                response = await client.LowLevel.SearchAsync<Stream>(client.ConnectionSettings.DefaultIndex, options.TypeName, json);
            }

            if (response.Success)
            {
                return await new StreamReader(response.Body).ReadToEndAsync();
            }

            return string.Empty;
        }
    }
}
