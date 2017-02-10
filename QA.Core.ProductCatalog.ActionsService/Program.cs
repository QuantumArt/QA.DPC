using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using QA.Validation.Xaml.Extensions.Rules;

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

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new ActionsService() 
            };
            ServiceBase.Run(ServicesToRun);
        }

        internal static RemoteValidationResult ProceedRemoteValidation()
        {
            return new RemoteValidationResult();
        }
    }
}
