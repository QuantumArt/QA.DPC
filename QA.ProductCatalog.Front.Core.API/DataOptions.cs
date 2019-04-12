﻿namespace QA.ProductCatalog.Front.Core.API
{
    public class DataOptions
    {
        public string FixedConnectionString { get; set; }
        
        public string DesignConnectionString { get; set; }
        
        public bool UsePostgres { get; set; }
        public bool UseProductVersions { get; set; }
        public string InstanceId { get; set; }
    }
}
