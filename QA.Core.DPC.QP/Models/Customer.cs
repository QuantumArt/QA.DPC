using QP.ConfigurationService.Models;

namespace QA.Core.DPC.QP.Models
{
    public class Customer
    {
        public string CustomerCode { get; set;}
        public DatabaseType DatabaseType { get; set;}
        public string ConnectionString { get; set; }
    }
}
