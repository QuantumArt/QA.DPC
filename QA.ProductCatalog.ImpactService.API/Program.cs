using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using NLog.Web;

namespace QA.ProductCatalog.ImpactService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .CaptureStartupErrors(true)
                .UseSetting("detailedErrors", "true")
                .UseNLog()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
