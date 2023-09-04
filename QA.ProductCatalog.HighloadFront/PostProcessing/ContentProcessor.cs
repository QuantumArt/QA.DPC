using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.PostProcessing
{
    public class ContentProcessor : IProductReadPostProcessor
    {
        private const string SourceJsonPath = "hits.hits[*]._source";
        private const int DefaultDepthToSearch = 4;

        public async Task<string> ReadSourceNodes(string input, ProductsOptionsBase options)
        {
            if (options.DataFilters.Any())
            {
                return PostProcess(input, options.DataFilters).ToString();
            }
            else
            {
                return await SimplePostProcess(input);
            }
        }

        private async Task<string> SimplePostProcess(string input, int depthToSearch = DefaultDepthToSearch)
        {
            var sb = new StringBuilder();
            await JsonFragmentExtractor.ExtractJsonFragment("_source", input, sb, depthToSearch);
            return sb.ToString();
        }

        private JArray PostProcess(string input, Dictionary<string, string> optionsDataFilters)
        {
            var hits = JObject.Parse(input).SelectTokens(SourceJsonPath).ToArray();

            foreach (var dataFilter in optionsDataFilters)
            {
                foreach (var hit in hits)
                {
                    var jArrays = hit.SelectTokens(dataFilter.Key).OfType<JArray>().ToArray();
                    foreach (var jArray in jArrays)
                    {
                        var relevantTokens = jArray.SelectTokens(dataFilter.Value).ToArray();
                        jArray.Clear();
                        foreach (var rToken in relevantTokens)
                        {
                            jArray.Add(rToken);
                        }
                    }
                }
            }

            return new JArray(hits);
        }
    }
}
