namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class DataOptions
    {
        public ElasticOption[] Elastic2 { get; set; }    
    }

    public class ElasticOption
    {
        public string Adress { get; set; }
        public string Index { get; set; }
        public string Language { get; set; }
        public string State { get; set; }
    }
}
