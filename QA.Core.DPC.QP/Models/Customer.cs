using QP.ConfigurationService.Models;
using C = Quantumart.QP8.Constants;

namespace QA.Core.DPC.QP.Models
{
    public class Customer
    {
        public string CustomerCode { get; set;}
        public DatabaseType DatabaseType { get; set;}
        public string ConnectionString { get; set; }
        public C.DatabaseType QpDatabaseType => (C.DatabaseType) (int) DatabaseType;
    }
}
