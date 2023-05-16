using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.PostProcessing
{
    public abstract class ExpandBaseProcessor
    {
        protected void CollectIds(HashSet<int> ids, JToken input, ProductsOptionsExpand options)
        {
            foreach (var token in input.SelectTokens(options.Path))
            {
                if (token is JArray)
                {
                    foreach (var item in token)
                    {
                        ids.Add(GetId(item));
                    }
                }
                else if (token is JObject)
                {
                    ids.Add(GetId(token));
                }
            }
        }

        protected int GetId(JToken token) => token["Id"].Value<int>();
    }
}
