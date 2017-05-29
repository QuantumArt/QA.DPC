using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Infrastructure;

namespace QA.ProductCatalog.HighloadFront.Models
{
    public class PushMessage
    {
        public JObject Product { get; set; }
        
        public RegionTag[] RegionTags { get; set; }
    }
}