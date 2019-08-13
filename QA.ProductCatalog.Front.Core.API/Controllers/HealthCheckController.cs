using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Npgsql;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QA.Core.DPC.QP.Models;
using QP.ConfigurationService.Models;
using Quantumart.QPublishing.Database;

namespace QA.ProductCatalog.Front.Core.API.Controllers
{
    [Route("api")]
    public class HealthCheckController : Controller
    {
        private DataOptions _options;
        private IntegrationProperties _intOptions;
        public HealthCheckController(DataOptions options, IOptions<IntegrationProperties> integrationProps)
        {
            _options = options;
            _intOptions = integrationProps.Value;
        }

        [HttpGet("healthcheck", Name="HealthCheck")]
        [HttpGet("{customerCode}/healthcheck", Name="HealthCheck-Consolidate")]
        public ActionResult Index()
        {

            var sb = new StringBuilder();
            sb.AppendLine("Application: OK");
            var dboKStr = IsDbConnected() ? "OK" : "Error";
            sb.AppendLine("Database: " + dboKStr);
            return Content(sb.ToString(), "text/plain");
        }

        private DbConnection GetDbConnection(CustomerConfiguration cc)
        {
            if (cc.DbType == DatabaseType.Postgres)
                return new NpgsqlConnection(cc.ConnectionString);
            return new SqlConnection(cc.ConnectionString);
        }

        private bool IsDbConnected()
        {
            DBConnector.ConfigServiceUrl = _intOptions.ConfigurationServiceUrl;
            DBConnector.ConfigServiceToken = _intOptions.ConfigurationServiceToken;
            var customerCode = ControllerContext.RouteData.Values["customerCode"];
            string connectionString = _options.FixedConnectionString;
            var cc = new CustomerConfiguration()
            {
                ConnectionString = connectionString,
                DbType = (_options.UsePostgres) ? DatabaseType.Postgres : DatabaseType.SqlServer
            };           
            if (string.IsNullOrEmpty(connectionString) && customerCode != null)
            {
                try
                {
                    cc = DBConnector.GetCustomerConfiguration(customerCode.ToString()).Result;
                }
                catch (Exception)
                {
                    return false;
                }    
            }

            if (string.IsNullOrEmpty(cc.ConnectionString))
            {
                return false;
            }
            
            using (var connection = GetDbConnection(cc))
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch (DbException)
                {
                    return false;
                }
            }
        }
     }
}
