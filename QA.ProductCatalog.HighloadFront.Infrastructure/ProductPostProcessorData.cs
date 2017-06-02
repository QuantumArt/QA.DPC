using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.ProductCatalog.HighloadFront.Infrastructure
{
    public class ProductPostProcessorData
    {
        public ProductPostProcessorData(JObject product)
            : this(product, null, DateTime.Now)
        {
        }

        public ProductPostProcessorData(JObject product, RegionTag[] regionTags, DateTime updated)
        {
            Product = product;
            RegionTags = regionTags;
            Updated = updated;
        }

        public JObject Product { get; set; }
        public RegionTag[] RegionTags { get; set; }
        public DateTime Updated { get; set; }
    }
}
