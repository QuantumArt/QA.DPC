using System;

namespace QA.Core.DPC.QP.Models
{
    public class ConnectionProperties
    {
        public ConnectionProperties()
        {
            UseQpMonitoring = true;
            QpMode = false;
            TransactionTimeout = TimeSpan.FromMinutes(10);
        }
        
        public string DpcConnectionString { get; set; }
        
        public string TasksConnectionString { get; set; }
        
        public string NotificationsConnectionString { get; set; }
        
        public string LiveMonitoringConnectionString { get; set; }
        
        public string StageMonitoringConnectionString { get; set; }
        
        public bool UseQpMonitoring { get; set; }
        
        public bool QpMode { get; set; }
        
        public TimeSpan TransactionTimeout { get; set; }
    }
}