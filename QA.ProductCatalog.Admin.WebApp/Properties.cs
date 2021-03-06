using System;

namespace QA.ProductCatalog.Admin.WebApp
{
    public class Properties
    {
        public Properties()
        {
            UserId = 1;
            WatcherInterval = TimeSpan.FromMinutes(1);
        }

        public string Version { get; set; }

        public string BuildVersion { get; set; }

        public int UserId { get; set; }
        
        public bool UseAuthorization { get; set; }
        
        public TimeSpan WatcherInterval { get; set; }
        
        public bool AutoRegisterConsolidationCache { get; set; }
        
        public string Name { get; set; }
        
    }
}