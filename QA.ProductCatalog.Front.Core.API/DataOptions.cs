﻿﻿namespace QA.ProductCatalog.Front.Core.API
{
    public class DataOptions
    {
        public int CommandTimeout { get; set; } = 60;
        
        public string FixedConnectionString { get; set; }
        
        public string FixedCustomerCode { get; set; }
        
        public string DesignConnectionString { get; set; }
        
        public bool UsePostgres { get; set; }
        public bool UseProductVersions { get; set; }
        public string InstanceId { get; set; }
        
        public string Name { get; set; }
    }
}
