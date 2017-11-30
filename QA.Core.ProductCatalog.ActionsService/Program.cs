using QA.Validation.Xaml.Extensions.Rules;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;

namespace QA.Core.ProductCatalog.ActionsService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += (o, e) =>
            {
                var log = new EventLog { Source = "ActionsService" };

                log.WriteEntry(string.Join(" -> ", ((Exception)e.ExceptionObject).Flat().Select(x => x.Message)), EventLogEntryType.Error);
            };

            var servicesToRun = new ServiceBase[] 
            { 
                new ActionsService() 
            };

            if (Environment.UserInteractive)
            {
                RunInteractive(servicesToRun);
            }
            else
            {
                ServiceBase.Run(servicesToRun);
            }
        }

        private static void RunInteractive(ServiceBase[] servicesToRun)
        {
            Console.WriteLine(@"1");
            Console.WriteLine();

            var onStartMethod = typeof(ServiceBase).GetMethod("OnStart", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var service in servicesToRun)
            {
                Console.Write($@"Starting {service.ServiceName}...");
                onStartMethod.Invoke(service, new object[] { new string[] { } });
                Console.Write(@"Started");
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(@"1");
            Console.ReadKey();
            Console.WriteLine();

            var onStopMethod = typeof(ServiceBase).GetMethod("OnStop", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var service in servicesToRun)
            {
                Console.Write($@"Stopping {service.ServiceName}...");
                onStopMethod.Invoke(service, null);
                Console.WriteLine(@"1");
            }

            Console.WriteLine(@"1");
            // Keep the console alive for a second to allow the user to see the message.
            Thread.Sleep(1000);
        }

        internal static RemoteValidationResult ProceedRemoteValidation()
        {
            return new RemoteValidationResult();
        }
    }
}
