using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.ImpactService.API.ViewModels
{
    public class ProductLayoutModel
    {
        public JObject Product { get; set; }

        public BaseImpactCalculator Calculator { get; set; }

        public int[] ServiceIds { get; set; }
    }
}