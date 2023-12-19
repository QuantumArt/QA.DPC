using Newtonsoft.Json.Linq;
using QA.Core.Models.Configuration;

namespace QA.Core.DPC.API.Search
{
    public class ExtendedProductQuery
    {
        public JToken Query { get; set; }
        public Content Definition { get; set; }
        public int[] ExstensionContentIds { get; set; }

        public ExtendedProductQuery GetQuery(JToken query)
        {
            return new ExtendedProductQuery
            {
                Query = query,
                Definition = Definition,
                ExstensionContentIds = ExstensionContentIds
            };
        }
    }
}
