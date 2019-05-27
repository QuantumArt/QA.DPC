using QA.ProductCatalog.ContentProviders;
using System;

namespace QA.ProductCatalog.HighloadFront.Options
{
    public class DataOptions
    {
        public DataOptions()
        {
            ElasticTimeout = 15;
            FailuresBeforeCircuitBreaking = 3;
            CircuitBreakingInterval = 60;
            VersionCaceExpiration = TimeSpan.FromMinutes(1);
        }
        
        public bool CanUpdate { get; set; }

        public string InstanceId { get; set; }

        public bool QpMode { get; set; }
        
        public string FixedCustomerCode { get; set; }

        public string FixedConnectionString { get; set; }

        public ElasticIndex[] Elastic { get; set; }

        public int ElasticTimeout { get; set; }

        public int FailuresBeforeCircuitBreaking { get; set; }

        public int CircuitBreakingInterval { get; set; }
        public TimeSpan VersionCaceExpiration { get; set; }
    }
}
