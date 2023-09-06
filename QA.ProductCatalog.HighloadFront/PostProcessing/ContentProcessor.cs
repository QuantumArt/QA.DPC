using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Options;
using System.Text.Json.Nodes;

namespace QA.ProductCatalog.HighloadFront.PostProcessing
{
    public class ContentProcessor : IProductReadPostProcessor
    {
        private const string SourceJsonPath = "hits.hits[*]._source";
        private const int DefaultDepthToSearch = 4;

        public async Task<string> ReadSourceNodes(string input, ProductsOptionsBase options)
        {
            var result = await SimplePostProcess(input);
            
            if (options.DataFilters.Any())
            {
                return PostProcess(result, options.DataFilters).ToJsonString(PostProcessHelper.GetSerializerOptions());
            }
            
            return result;
        }

        private async Task<string> SimplePostProcess(string input, int depthToSearch = DefaultDepthToSearch)
        {
            var sb = new StringBuilder();
            await JsonFragmentExtractor.ExtractJsonFragment("_source", input, sb, depthToSearch);
            return sb.ToString();
        }

        private JsonArray PostProcess(string input, Dictionary<string, string> optionsDataFilters)
        {
            var elements = JsonNode.Parse(input).AsArray();
            foreach (var dataFilter in optionsDataFilters)
            {
                foreach (var element in elements)
                {
                    var jArrays = PostProcessHelper.Select(element, dataFilter.Key);
                    foreach (var jArray in jArrays)
                    {
                        var relevantTokens = PostProcessHelper.Select(jArray, dataFilter.Value);
                        jArray.AsArray().Clear();
                        foreach (var rToken in relevantTokens)
                        {
                            jArray.AsArray().Add(rToken);
                        }
                    }
                }
            }
            return elements;
        }
    }
}
