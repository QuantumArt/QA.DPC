using System;
using System.Data.SqlClient;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Quantumart.QPublishing.Database;

namespace QA.ProductCatalog.Front.Core.API.Controllers
{
    [Route("api/{customerCode}/HealthCheck")]
    public class HealthCheckController : Controller
    {
        private DataOptions _options;
        public HealthCheckController(IOptions<DataOptions> options)
        {
            _options = options.Value;
        }

        [HttpGet]
        public ActionResult Index(string customerCode)
        {

            var sb = new StringBuilder();
            sb.AppendLine("Application: OK");
            var dboKStr = IsDbConnected(customerCode) ? "OK" : "Error";
            sb.AppendLine("Database: " + dboKStr);
            return Content(sb.ToString(), "text/plain");
        }

        private bool IsDbConnected(string customerCode)
        {
            string connectionString = _options.FixedConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                try
                {
                    connectionString = DBConnector.GetConnectionString(customerCode);
                }
                catch (Exception)
                {
                    return false;
                }
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
