using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.PostProcessing
{
    public class ExpandReadProcessor : ExpandBaseProcessor, IProductReadExpandPostProcessor
    {
        public int[] GetExpandIds(JToken input, ProductsOptionsExpand options)
        {
            var ids = new HashSet<int>();

            if (input is JArray)
            {
                foreach (var item in input)
                {
                    CollectIds(ids, item, options);
                }
            }
            else
            {
                CollectIds(ids, input, options);
            }

            return ids.ToArray();
        }
    }
}
