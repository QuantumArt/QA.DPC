using System.Linq;

namespace QA.ProductCatalog.ImpactService.API
{
    public class ConfigurationOptions
    {
        public ConfigurationOptions()
        {
            ElasticBaseAddress = "http://localhost:9200";
            RootRegionId = 19975;
        }


        public ElasticIndex[] ElasticIndexes { get; set; }

        public string ElasticBaseAddress { get; set; }

        public int RootRegionId { get; set; }

        public string GetExactIndexName(string state, string language)
        {
            return ElasticIndexes?.Where(n => n.State == state && n.Language == language).Select(n => n.Name).SingleOrDefault();
        }

        public string GetIndexName(string state, string language)
        {
            return GetExactIndexName(state, language) ?? ElasticIndex.DefaultName;
        }

    }
}