using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.ImpactService.API.Services
{
    public class ElasticSearchRepository : ISearchRepository
    {
        private string _sourceQuery = "hits.hits.[?(@._source)]._source";


        private Regex GetRegionTagRegex(string tag)
        {
            return new Regex($@"[<\[]replacement[>\]]tag={tag}[<\[]/replacement[>\]]");
        }

        public async Task<DateTimeOffset> GetLastUpdated(int[] productIds, SearchOptions options)
        {
            var result = await GetContent(GetJsonQuery(productIds, true), options);
            var dates = JObject.Parse(result).SelectTokens("hits.hits.[?(@.UpdateDate)].UpdateDate").Select(n => DateTimeOffset.Parse(n.ToString() ,CultureInfo.InvariantCulture)).ToArray();
            //if (dates.Length < productIds.Length)
            //    throw new ApplicationException("Some products not found");
            return dates.Max();
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

        
        public async Task<JObject[]> GetProducts(int[] productIds, SearchOptions options)
        {
            var result = await GetContent(GetJsonQuery(productIds), options);

            result = await ProcessRegionTags(productIds, result, options);

            var hits = JObject.Parse(result).SelectTokens(_sourceQuery).ToArray();

            return hits.Select(n => (JObject)n).ToArray();
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

        public async Task<string> GetContent(string json, SearchOptions options)
        {
            var result = string.Empty;
            using (var client = new HttpClient())
            {
                var address = $"{options.BaseAddress}/{options.IndexName}/_search";
                if (options.TypeName != null)
                {
                    address = $"{options.BaseAddress}/{options.IndexName}/{options.TypeName}/_search";
                }
                client.BaseAddress = new Uri(address);
                client.DefaultRequestHeaders.Accept.Clear();
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(address, httpContent);
             
                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsStringAsync();
                }
            }
            return result;
        }
    }
}
