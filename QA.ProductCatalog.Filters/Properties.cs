namespace QA.ProductCatalog.Filters
{
    public class Properties
    {
        public Properties()
        {
            UserId = 1;
            WatcherInterval = TimeSpan.FromMinutes(1);
        }

        public int UserId { get; set; }

        public bool UseAuthorization { get; set; }

        public TimeSpan WatcherInterval { get; set; }

        public bool AutoRegisterConsolidationCache { get; set; }

        public string Name { get; set; }

        public string DocumentLicenceKey { get; set; }
    }
}
