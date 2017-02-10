using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Web.Mvc;

namespace QA.ProductCatalog.SiteSyncWebHost.Controllers
{
    public class HealthCheckController : Controller
    {

        public ActionResult Index()
        {

            var sb = new StringBuilder();
            sb.AppendLine("Application: OK");
            var connectionString = ConfigurationManager.ConnectionStrings["QA.Core.DPC.Integration.Properties.Settings.dpc_webConnectionString"].ToString();
            var dboKStr = IsServerConnected(connectionString) ? "OK" : "Error";
            sb.AppendLine("Database: " + dboKStr);
            return Content(sb.ToString(), "text/plain");
        }

        private static bool IsServerConnected(string connectionString)
        {
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