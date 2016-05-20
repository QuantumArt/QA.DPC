using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.HighloadFront.Models
{
    public class PushMessage
    {
        public JObject Product { get; set; }
        
        //они погут прийти но не нужны
        //public RegionTag[] RegionTags { get; set; }
    }
}