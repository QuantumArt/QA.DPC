using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using NLog.Web;

namespace QA.ProductCatalog.HighloadFront.Core.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
#if DEBUG
                .UseUrls("http://*:9092;http://localhost:5000/")
#endif
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .CaptureStartupErrors(true)
                .UseSetting("detailedErrors", "true")
                .UseStartup<Startup>()
                .UseNLog()
                .UseApplicationInsights()
                .Build();

            host.Run();
        }
    }
}
