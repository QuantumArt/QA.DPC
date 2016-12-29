using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Infrastructure;

namespace QA.ProductCatalog.HighloadFront.Importer
{
    public class DpcResponse
    {
        public JObject Product { get; set; }

        public RegionTag[] RegionTags { get; set; }
    }
}