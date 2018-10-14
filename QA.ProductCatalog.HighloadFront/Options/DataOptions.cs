﻿using QA.ProductCatalog.ContentProviders;

namespace QA.ProductCatalog.HighloadFront.Options
{
    public class DataOptions
    {
        public bool CanUpdate { get; set; }

        public string InstanceId { get; set; }

        public bool QpMode { get; set; }
        
        public string FixedCustomerCode { get; set; }

        public string FixedConnectionString { get; set; }

        public ElasticIndex[] Elastic { get; set; }

        public int ElasticTimeout { get; set; }
    }
}
