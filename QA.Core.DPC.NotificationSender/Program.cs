using System;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;

namespace QA.Core.DPC
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            var servicesToRun = new ServiceBase[]
            {
                new NotificationSender()
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
    }
}
