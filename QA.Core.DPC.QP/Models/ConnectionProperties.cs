using System;

namespace QA.Core.DPC.QP.Models
{
    public class ConnectionProperties
    {
        public ConnectionProperties()
        {
            UseQpMonitoring = true;
            QpMode = false;
            UsePostgres = false;
            TransactionTimeout = TimeSpan.FromMinutes(3);
        }
        
        public string DpcConnectionString { get; set; }
        
        public string TasksConnectionString { get; set; }
        
        public string DesignConnectionString { get; set; }
        
        public string NotificationsConnectionString { get; set; }
        
        public string LiveMonitoringConnectionString { get; set; }
        
        public string StageMonitoringConnectionString { get; set; }
        
        public bool UseQpMonitoring { get; set; }
        
        public bool QpMode { get; set; }
        
        public bool UsePostgres { get; set; }
        
        public TimeSpan TransactionTimeout { get; set; }
    }
}