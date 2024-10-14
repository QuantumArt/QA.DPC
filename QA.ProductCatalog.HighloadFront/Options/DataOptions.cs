using QA.ProductCatalog.ContentProviders;
using System;
using QP.ConfigurationService.Models;

namespace QA.ProductCatalog.HighloadFront.Options
{
    public class DataOptions
    {
        public DataOptions()
        {
            ElasticTimeout = 15;
            FailuresBeforeCircuitBreaking = 3;
            CircuitBreakingInterval = 60;
            VersionCacheExpiration = TimeSpan.FromMinutes(1);
            CleanKeysOptions = new CleanKeysOptions();
            UseAliases = true;
        }
        
        public bool CanUpdate { get; set; }
        
        public bool UseAliases { get; set; }

        public string InstanceId { get; set; }

        public bool QpMode { get; set; }
        
        public int SettingsContentId { get; set; }
        
        public string FixedCustomerCode { get; set; }

        public string FixedConnectionString { get; set; }
        
        public bool UsePostgres { get; set; }

        public ElasticIndex[] Elastic { get; set; }

        public int ElasticTimeout { get; set; }

        public int FailuresBeforeCircuitBreaking { get; set; }

        public int CircuitBreakingInterval { get; set; }
        public TimeSpan VersionCacheExpiration { get; set; }
        
        public CleanKeysOptions CleanKeysOptions { get; set; }
        
        public DatabaseType GetDatabaseType()
        {
            return UsePostgres ? DatabaseType.Postgres : DatabaseType.SqlServer;
        }
    }
}
