using System;
using System.Data.SqlClient;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Quantumart.QPublishing.Database;

namespace QA.ProductCatalog.Front.Core.API.Controllers
{
    [Route("api")]
    public class HealthCheckController : Controller
    {
        private DataOptions _options;
        public HealthCheckController(IOptions<DataOptions> options)
        {
            _options = options.Value;
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

        private bool IsDbConnected()
        {
            var customerCode = ControllerContext.RouteData.Values["customerCode"];
            string connectionString = _options.FixedConnectionString;
            if (string.IsNullOrEmpty(connectionString) && customerCode != null)
            {
                try
                {
                    connectionString = DBConnector.GetConnectionString(customerCode.ToString());
                }
                catch (Exception)
                {
                    return false;
                }    
            }
            
            if (string.IsNullOrEmpty(connectionString))
            {
                return false;
            }
            
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch (SqlException)
                {
                    return false;
                }
            }
        }
     }
}
