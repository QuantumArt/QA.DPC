using System;
using System.Data.SqlClient;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QPublishing.Database;

namespace QA.ProductCatalog.Front.Core.API.Controllers
{
    [Route("api/HealthCheck")]
    public class HealthCheckController : Controller
    {
        [HttpGet("{customerCode}")]
        public ActionResult Index(string customerCode)
        {

            var sb = new StringBuilder();
            sb.AppendLine("Application: OK");
            var dboKStr = IsDbConnected(customerCode) ? "OK" : "Error";
            sb.AppendLine("Database: " + dboKStr);
            return Content(sb.ToString(), "text/plain");
        }

        private static bool IsDbConnected(string customerCode)
        {
            string connectionString;
            try
            {
                connectionString = DBConnector.GetConnectionString(customerCode);
            }
            catch (Exception)
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
