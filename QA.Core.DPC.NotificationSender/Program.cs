﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using NLog.Web;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Unity.Microsoft.DependencyInjection;

namespace QA.Core.DPC
{
    public class Program
    {
        public static void Main(string[] args)
        {

            NLog.LogManager.LoadConfiguration("NLogClient.config");
                
            AppDomain.CurrentDomain.UnhandledException += (o, e) =>
            {
                var log = new EventLog { Source = "ActionsService" };

                log.WriteEntry(string.Join(" -> ", ((Exception)e.ExceptionObject).Flat().Select(x => x.Message)), EventLogEntryType.Error);
            };
            
            var isService = !(Debugger.IsAttached || ((IList) args).Contains("--console"));
            if (isService)
            {
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                Directory.SetCurrentDirectory(pathToContentRoot);
            }

            var host = BuildWebHost(args);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (isService)
                {
                    host.RunAsCustomService();
                }
                else
                {
                    host.Run();
                }

            }
            else
            {
                host.Run();
            }
        }

        private static IWebHost BuildWebHost(string[] args)
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
