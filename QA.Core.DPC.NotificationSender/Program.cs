﻿using System;
using System.Diagnostics;
using System.Linq;
 using System.IO;
 using Microsoft.Extensions.Logging;
using NLog.Web;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Unity.Microsoft.DependencyInjection;
using QA.DPC.Core.Helpers;

namespace QA.Core.DPC
{
    public static class Program
    {
        public static void Main(string[] args)
        {

            NLog.LogManager.LoadConfiguration("NLogClient.config");
                
            AppDomain.CurrentDomain.UnhandledException += (o, e) =>
            {
                var log = new EventLog { Source = "NotificationSender" };

                log.WriteEntry(string.Join(" -> ", ((Exception)e.ExceptionObject).Flat().Select(x => x.Message)), EventLogEntryType.Error);
            };

            args.SetDirectory().BuildWebHost().RunAdaptive(args);
        }

        private static IWebHost BuildWebHost(this string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hosting.json", optional: true, reloadOnChange: true)
                .Build();
            
            var builder = WebHost.CreateDefaultBuilder(args)
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
                    .UseNLog()
                    .UseUnityServiceProvider()
                    .UseStartup<Startup>()
                ;

            return builder.Build();
        }
    }
}
