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
            : this(product, null, DateTime.Now, DateTime.Now)
        {
        }

        public ProductPostProcessorData(JObject product, RegionTag[] regionTags, DateTime created, DateTime updated)
        {
            Product = product;
            RegionTags = regionTags;
            Created = created;
            Updated = updated;
        }

        public JObject Product { get; set; }
        public RegionTag[] RegionTags { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}
