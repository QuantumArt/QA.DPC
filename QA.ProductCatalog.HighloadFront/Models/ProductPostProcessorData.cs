using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace QA.ProductCatalog.HighloadFront.Models
{
    public class ProductPostProcessorData
    {
        public ProductPostProcessorData(JsonElement product)
            : this(product, new RegionTag[] {}, DateTime.Now)
        {
        }

        public ProductPostProcessorData(JsonElement product, RegionTag[] regionTags, DateTime updated)
        {
            Product = JsonObject.Create(product);
            RegionTags = regionTags ?? throw new ArgumentNullException(nameof(regionTags));
            Updated = updated;
        }

        public JsonObject Product { get; set; }
        public RegionTag[] RegionTags { get; set; }
        public DateTime Updated { get; set; }
    }
}
