namespace QA.ProductCatalog.TmForum.Models
{
    public class TmfSettings
    {
        public bool IsEnabled { get; set; }
        public int DefaultLimit { get; set; } = 10;
        public bool IsLive { get; set; }
    }
}
