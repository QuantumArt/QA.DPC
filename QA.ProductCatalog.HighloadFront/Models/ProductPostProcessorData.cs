using System;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.HighloadFront.Models
{
    public class ProductPostProcessorData
    {
        public ProductPostProcessorData(JObject product)
            : this(product, null, DateTime.Now)
        {
        }

        public ProductPostProcessorData(JObject product, RegionTag[] regionTags, DateTime updated)
        {
            Product = product ?? throw new ArgumentNullException(nameof(product));
            RegionTags = regionTags ?? throw new ArgumentNullException(nameof(regionTags));
            Updated = updated;
        }

        public JObject Product { get; set; }
        public RegionTag[] RegionTags { get; set; }
        public DateTime Updated { get; set; }
    }
}
