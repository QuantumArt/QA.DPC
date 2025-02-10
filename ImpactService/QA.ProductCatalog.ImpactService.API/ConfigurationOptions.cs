using System.Linq;

namespace QA.ProductCatalog.ImpactService.API
{
    public class ConfigurationOptions
    {
        public ConfigurationOptions()
        {
            ElasticBaseAddress = "http://localhost:9200";
            RootRegionId = 19975;
            CachingInterval = 300;
            NegativeCachingInterval = 20;
            LoadDefaultServices = true;
            HttpTimeout = 15;
            FailuresBeforeCircuitBreaking = 3;
            CircuitBreakingInterval = 60;
        }

        private string[] _elasticUrls;

        public string[] ExtraLibraries { get; set; }
        
        public ElasticIndex[] ElasticIndexes { get; set; }

        public bool ConsolidateCallGroupsForIcin { get; set; }

        public bool LoadDefaultServices { get; set; }

        public string ElasticBaseAddress { get; set; }

        public string ElasticBasicToken { get; set; }

        public string[] ElasticUrls
        {
            get
            {
                if (_elasticUrls != null) return _elasticUrls;
                _elasticUrls = ElasticBaseAddress.Split(';').Select(n => n.Trim()).ToArray();
                return _elasticUrls;
            }
        }

        public int CachingInterval { get; set; }

        public int NegativeCachingInterval { get; set; }
        
        public int HttpTimeout { get; set; }
        
        public int FailuresBeforeCircuitBreaking { get; set; }
        
        public int CircuitBreakingInterval { get; set; }

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