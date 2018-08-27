using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.ImpactService.API.Services
{
    public class SearchOptions
    {
        public string HomeRegion { get; set; }
        
        public JObject HomeRegionData { get; set; }
        
        public string BaseAddress { get; set; }

        public string IndexName { get; set; }

        public string TypeName { get; set; }

        public SearchOptions Clone()
        {
            var result = new SearchOptions
            {
                HomeRegion = HomeRegion,
                TypeName = TypeName,
                BaseAddress = BaseAddress,
                IndexName = IndexName,
                HomeRegionData = (JObject)HomeRegionData?.DeepClone()
            };
            return result;

        }


    }
}