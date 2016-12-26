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
        private string baseAddress = "http://mscnosql01:9200/products/_search";

        private int _homeRegionId;
        private string _sourceQuery = "hits.hits.[?(@._source)]._source";


        private Regex GetRegionTagRegex(string tag)
        {
            return new Regex($@"[<\[]replacement[>\]]tag={tag}[<\[]/replacement[>\]]");
        }

        public async Task<DateTimeOffset> GetLastUpdated(int[] productIds)
        {
            var result = await GetContent(GetJsonQuery(productIds, true));
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

        public async Task<JObject[]> GetProducts(int[] productIds)
        {
            var result = await GetContent(GetJsonQuery(productIds));

            result = await ProcessRegionTags(productIds, result);

            var hits = JObject.Parse(result).SelectTokens(_sourceQuery).ToArray();

            return hits.Select(n => (JObject)n).ToArray();
        }

        private async Task<string> ProcessRegionTags(int[] productIds, string input)
        {
            var result = input;
            var tagsToProcess = await GetTagsToProcess(productIds);
            for (var i = 1; i <= 2; i++)
            {
                foreach (var tag in tagsToProcess)
                {
                    result = GetRegionTagRegex(tag.Key).Replace(result, tag.Value);
                }
            }
            return result;
        }

        private async Task<Dictionary<string, string>> GetTagsToProcess(int[] productIds)
        {
            var regionTagIds = productIds.Select(n => 0 - n).ToArray();
            var tagResult = await GetContent(GetJsonQuery(regionTagIds));
            var tags =
                JObject.Parse(tagResult)
                    .SelectTokens(_sourceQuery)
                    .SelectMany(n => (JArray)n.SelectToken("RegionTags"))
                    .ToArray();
            var tagsToProcess = new Dictionary<string, string>();

            foreach (var tag in tags)
            {
                var title = tag["Title"].ToString();
                if (tagsToProcess.ContainsKey(title)) continue;
                var value = String.Empty;
                if (_homeRegionId == 0)
                {
                    value = tag["Values"][0]["Value"].ToString();
                }
                else
                {
                    value =
                        tag
                            .SelectTokens("[?(@.Title]")
                            .SingleOrDefault(n => ((JArray) n["RegionsId"]).Any(m => (int) m == _homeRegionId))?
                            .ToString();
                }
                value = JsonConvert.ToString(value);
                tagsToProcess.Add(title, value.Substring(1, value.Length - 2));
            }
            return tagsToProcess;
        }

        public void SetHomeRegionId(int homeRegionId)
        {
            _homeRegionId = homeRegionId;
        }


        public async Task<string> GetContent(string json)
        {
            var result = String.Empty;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(baseAddress, httpContent);
                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsStringAsync();
                }
            }
            return result;
        }
    }
}
