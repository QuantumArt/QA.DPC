using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace QA.ProductCatalog.HighloadFront.Core.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHost(builder =>
                {
                    builder
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
                        .UseKestrel((builderContext, options) =>
                        {
                            options.Configure(builderContext.Configuration.GetSection("Kestrel"), reloadOnChange: true);
                        })
                        .UseStartup<Startup>()
                        .UseNLog();
                });
    }
}