using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using Unity;
using Unity.Microsoft.DependencyInjection;

namespace QA.ProductCatalog.Admin.WebApp
{
    public class Program
    {
        private static IUnityContainer _container;
        public static void Main(string[] args)
        {

            NLog.LogManager.LoadConfiguration("NLogClient.config");
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            _container = new UnityContainer();
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
                        .UseStartup<Startup>();
                    })
                    .ConfigureAppConfiguration((context, config) =>
                    {
                    config.Build();
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
                    }).UseNLog();

            return builder;
        }
    }
}
