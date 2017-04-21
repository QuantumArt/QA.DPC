using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.ImpactService.API.Controllers
{
    public class ProductLayoutModel
    {
        public JObject Product { get; set; }

        public BaseImpactCalculator Calculator { get; set; }

        public int[] ServiceIds { get; set; }

        public string State { get; set; }

        public string Language { get; set; }

        public string Region { get; set; }

        public string HomeRegion { get; set; }

        public string Country { get; set;  }
    }
}