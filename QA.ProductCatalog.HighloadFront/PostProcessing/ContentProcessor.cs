using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Options;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.ProductCatalog.HighloadFront.PostProcessing
{
    public class ContentProcessor : IProductReadPostProcessor
    {
        public async Task<string> GetResult(string input, ProductsOptionsBase options)
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

        private async Task<string> SimplePostProcess(string text, bool filter = true)
        {
            var result = text;

            if (filter)
            {
                var sb = new StringBuilder();
                await JsonFragmentExtractor.ExtractJsonFragment("_source", text, sb, 4);
                result = sb.ToString();
            }

            return result;
        }

        private JArray PostProcess(string input, Dictionary<string, string> optionsDataFilters)
        {
            const string sourceQuery = "hits.hits.[?(@._source)]._source";
            var hits = JObject.Parse(input).SelectTokens(sourceQuery).ToArray();
            foreach (var df in optionsDataFilters)
            {
                foreach (var hit in hits)
                {
                    var jArrays = hit.SelectTokens(df.Key).OfType<JArray>().ToArray();
                    foreach (var jArray in jArrays)
                    {
                        var relevantTokens = jArray.SelectTokens(df.Value).ToArray();
                        jArray.Clear();
                        foreach (var rToken in relevantTokens)
                        {
                            jArray.Add(rToken);
                        }
                    }
                }
            }

            return new JArray(hits.Select(n => (object)n));
        }

        public string Expand(string input, JArray expanded, ProductsOptionsExpand options)
        {
            var expandDictionary = expanded.ToDictionary(x => GetId(x));

            if (!expandDictionary.Any())
            {
                return input;
            }

            var token = JToken.Parse(input);

            if (token is JArray)
            {
                foreach (var item in token.ToArray())
                {
                    Expand(expandDictionary, item, options);
                }
            }
            else
            {
                Expand(expandDictionary, token, options);
            }


            return token.ToString();
        }

        private void Expand(Dictionary<int, JToken> expandDictionary, JToken product, ProductsOptionsExpand options)
        {
            if (string.IsNullOrEmpty(options.Name))
            {
                ExpandInPlace(expandDictionary, product, options);
            }
            else
            {
                ExpandInField(expandDictionary, product, options);
            }
        }

        private void ExpandInPlace(Dictionary<int, JToken> expandDictionary, JToken product, ProductsOptionsExpand options)
        {
            foreach (var token in product.SelectTokens(options.Path).ToArray())
            {
                if (token is JArray)
                {
                    foreach (var item in token.ToArray())
                    {
                        AddExpandValue(expandDictionary, item);
                    }
                }
                if (token is JObject)
                {
                    AddExpandValue(expandDictionary, token);
                }
            }
        }

        private void ExpandInField(Dictionary<int, JToken> expandDictionary, JToken product, ProductsOptionsExpand options)
        {
            var ids = new HashSet<int>();
            UpdateExpandIds(ids, product, options);
            var values = ids.Where(id => expandDictionary.ContainsKey(id)).Select(id => expandDictionary[id]).ToArray();
            var valuesObject = new JArray(values);
            product[options.Name] = valuesObject;
        }

        private void AddExpandValue(Dictionary<int, JToken> expandDictionary, JToken expandable)
        {
            if (expandDictionary.TryGetValue(GetId(expandable), out var value))
            {
                expandable.Replace(value);
            }            
        }

        public JArray GetExpand(string input)
        {
            const string sourceQuery = "hits.hits.[?(@._source)]._source";
            var hits = JObject.Parse(input).SelectTokens(sourceQuery);
            return new JArray(hits);
        }

        public int[] GetExpandIds(string input, ProductsOptionsExpand options)
        {
            var ids = new HashSet<int>();
            var token = JToken.Parse(input);

            if (token is JArray)
            {
                foreach(var item in token)
                {
                    UpdateExpandIds(ids, item, options);
                }
            }
            else
            {
                UpdateExpandIds(ids, token, options);
            }

            return ids.ToArray();
        }

        private void UpdateExpandIds(HashSet<int> ids, JToken product, ProductsOptionsExpand options)
        {
            foreach (var token in product.SelectTokens(options.Path))
            {
                if (token is JArray)
                {
                    foreach (var item in token)
                    {
                        ids.Add(GetId(item));
                    }
                }
                if (token is JObject)
                {
                    ids.Add(GetId(token));
                }
            }
        }

        private int GetId(JToken token) => token["Id"].Value<int>();
    }
}
