using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using QA.Core.DPC.QP.Models;
using QP.ConfigurationService.Models;
using Quantumart.QPublishing.Database;

namespace QA.ProductCatalog.Front.Core.API
{
    
    public class ConnectionService
    {
        private readonly HttpContext _context;

        private readonly DataOptions _options;
        
        public ConnectionService(IHttpContextAccessor httpContextAccessor, DataOptions options)
        {
            _context = httpContextAccessor.HttpContext;
            _options = options;
        }

        public async Task<Customer> GetCustomer()
        {
            string customerCode = _context.GetRouteData().Values.ContainsKey("customerCode") 
                ? _context.GetRouteData().Values["customerCode"].ToString() 
                : _context.Request.Query["customerCode"].FirstOrDefault();
            if (customerCode == null)
            {
                return new Customer
                {
                    ConnectionString = _options.DesignConnectionString ?? _options.FixedConnectionString,
                    CustomerCode = customerCode,
                    DatabaseType = _options.UsePostgres ? DatabaseType.Postgres : DatabaseType.SqlServer
                };
            }

            var customer = await DBConnector.GetCustomerConfiguration(customerCode);
            return new Customer
            {
                ConnectionString = customer.ConnectionString,
                CustomerCode = customer.Name,
                DatabaseType = customer.DbType
            };
        }
        
        
    }
}