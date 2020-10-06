using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QP.ConfigurationService.Models;
using Quantumart.QPublishing.Database;

namespace QA.ProductCatalog.Front.Core.API
{
    
    public class ConnectionService
    {
        private readonly HttpContext _context;

        private readonly DataOptions _options;

        private readonly IntegrationProperties _intOptions;
        
        public ConnectionService(IHttpContextAccessor httpContextAccessor, DataOptions options, IOptions<IntegrationProperties> intOptions)
        {
            _context = httpContextAccessor.HttpContext;
            _options = options;
            _intOptions = intOptions.Value;
        }

        public async Task<Customer> GetCustomer()
        {
            var connectionString = _options.DesignConnectionString ?? _options.FixedConnectionString;
            
            var customerCode = _context.GetRouteData().Values.ContainsKey("customerCode") 
                ? _context.GetRouteData().Values["customerCode"].ToString() 
                : _context.Request.Query["customerCode"].FirstOrDefault();
            
            if (string.IsNullOrEmpty(connectionString) && string.IsNullOrWhiteSpace(customerCode))
            {
                throw new ApplicationException("Customer code or connection string is not defined");
            }
            
            if (!string.IsNullOrEmpty(connectionString))
            {
                return new Customer
                {
                    ConnectionString = connectionString,
                    CustomerCode = _options.FixedCustomerCode,
                    DatabaseType = _options.UsePostgres ? DatabaseType.Postgres : DatabaseType.SqlServer
                };
            }

            DBConnector.ConfigServiceUrl = _intOptions.ConfigurationServiceUrl;
            DBConnector.ConfigServiceToken = _intOptions.ConfigurationServiceToken;

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