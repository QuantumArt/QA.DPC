using System;
using NLog;
using QP.ConfigurationService.Models;
using Quantumart.QPublishing.Database;
using C = Quantumart.QP8.Constants;

namespace QA.Core.DPC.QP.Models
{
    public class Customer
    {
        private readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();
        
        public Customer(string connectionString, string customerCode, DatabaseType dbType)
        {
            ConnectionString = connectionString;
            CustomerCode = customerCode;
            DatabaseType = dbType;
            DbConnector = new DBConnector(ConnectionString, DatabaseType);
            IsConsolidated = false;
            EnableS3ForTenant = true;
        }
        public Customer(CustomerConfiguration config)
        {
            ConnectionString = config.ConnectionString.Replace("Provider=SQLOLEDB;", "");
            CustomerCode = config.Name;
            DatabaseType = config.DbType;
            DbConnector = new DBConnector(ConnectionString, DatabaseType);
            UpdateIsConsolidated();
        }
        
        public string CustomerCode { get; set;}
        public DatabaseType DatabaseType { get; set;}
        public string ConnectionString { get; set; }
        public DBConnector DbConnector { get; set; }
        public bool IsConsolidated { get; set; }
        public bool EnableS3ForTenant { get; set; }
        public C.DatabaseType QpDatabaseType => (C.DatabaseType) (int) DatabaseType;

        private void UpdateIsConsolidated()
        {
            try
            {
                var command = DbConnector.CreateDbCommand($"SELECT USE_DPC, USE_S3 FROM DB {DbConnector.WithNoLock}");
                var data = DbConnector.GetRealData(command);
                IsConsolidated = (bool)data.Rows[0]["USE_DPC"];
                EnableS3ForTenant = (bool)data.Rows[0]["USE_S3"];
            }
            catch(Exception)
            {
                IsConsolidated = false;
                EnableS3ForTenant = false;
                _logger.Error(() => $"Customer code {CustomerCode} is not accessible");
            }
        }   
    }
}
