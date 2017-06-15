using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace QA.ProductCatalog.Front.Core.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .CaptureStartupErrors(true)
                .UseSetting("detailedErrors", "true")
                .UseApplicationInsights()
                .Build();

            host.Run();
        }
    }
}
