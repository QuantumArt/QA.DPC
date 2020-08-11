using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace QA.ProductCatalog.FileSyncWebHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            NLog.LogManager.LoadConfiguration("NLogClient.config");
            
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hosting.json", optional: true, reloadOnChange: true)
                .Build();
            WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(config)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    if (hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        logging.AddConsole();
                        logging.AddDebug();
                    }
                })
                .ConfigureKestrel(serverOpts => {
                    serverOpts.AllowSynchronousIO = true;
                })
                .UseStartup<Startup>()
                .UseNLog()
                .Build()
                .Run();
        }
    }
}