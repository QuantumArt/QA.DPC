namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class DataOptions
    {
        public ElasticOption[] Elastic { get; set; } 
        
        public bool CanUpdate { get; set; }   
    }

    public class ElasticOption
    {
        public string Adress { get; set; }
        public string Index { get; set; }
        public string Language { get; set; }
        public string State { get; set; }
        public bool IsDefault { get; set; }
    }
}
