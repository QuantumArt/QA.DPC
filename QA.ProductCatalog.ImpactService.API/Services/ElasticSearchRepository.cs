using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.ImpactService.API.Services
{
    public class ElasticSearchRepository : ISearchRepository
    {
        private string baseAddress = "http://mscnosql01:9200/products/_search";


        public async Task<DateTimeOffset> GetLastUpdated(int[] productIds)
        {
            var result = await GetContent(GetJsonQuery(productIds, false));
            var dates = JObject.Parse(result).SelectTokens("hits.hits.[?(@.UpdateDate)].UpdateDate").Select(n => DateTimeOffset.Parse(n.ToString() ,CultureInfo.InvariantCulture)).ToArray();
            //if (dates.Length < productIds.Length)
            //    throw new ApplicationException("Some products not found");
            return dates.Max();
        }

        private string GetJsonQuery(int[] productIds, bool onlyModified)
        {
            var ids = string.Join(", ", productIds.Select(n => $@"""{n.ToString()}""").ToArray());
            var fieldsFilter = (onlyModified) ? @"_source: [""UpdateDate""]," : "";
            var query = $@"{{ {fieldsFilter} ""query"" : {{ ""ids"" : {{ ""values"" : [{ids}] }}}}}}";
            return query;
        }

        public async Task<JObject[]> GetProducts(int[] productIds)
        {
            var result = await GetContent(GetJsonQuery(productIds, false));
            var hits = JObject.Parse(result).SelectTokens("hits.hits.[?(@._source)]._source").ToArray();
            //if (hits.Length < productIds.Length)
            //    throw new ApplicationException("Some products not found");
            return hits.Select(n => (JObject)n).ToArray();
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
