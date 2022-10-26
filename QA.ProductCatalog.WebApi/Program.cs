using System;
using System.Diagnostics;
using System.Linq;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Unity.Microsoft.DependencyInjection;
using Unity;
using NLog.Web;

namespace QA.ProductCatalog.WebApi
{
    public class Program
    {
        private static IUnityContainer _container;

        public static void Main(string[] args)
        {
            _container = new UnityContainer();
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            NLog.LogManager.LoadConfiguration("NLogClient.config");
            BuildWebHost(args).Build().Run();
        }

        private static IHostBuilder BuildWebHost(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hosting.json", optional: true, reloadOnChange: true);
            
            var builder = Host.CreateDefaultBuilder(args)
                    .UseUnityServiceProvider(_container)
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder
                        .UseConfiguration(config.Build())
                        .UseStartup<Startup>();
                    })
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
                    .UseNLog()
                ;

            return builder;
        }
    }
}
