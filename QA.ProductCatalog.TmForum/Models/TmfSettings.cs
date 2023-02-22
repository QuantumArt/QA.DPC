namespace QA.ProductCatalog.TmForum.Models
{
    public class TmfSettings
    {
        public bool IsEnabled { get; set; }
        public int DefaultLimit { get; set; } = 10;
        public bool IsLive { get; set; }
        public bool SendProductsToKafka { get; set; }
        public string CreatedProductsTopic { get; set; }
        public string UpdatedProductsTopic { get; set; }
    }
}
