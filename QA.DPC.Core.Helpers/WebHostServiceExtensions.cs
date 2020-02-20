using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace QA.DPC.Core.Helpers
{
    public static class WebHostServiceExtensions
    {
        public static string[] SetDirectory(this string[] args)
        {
            if (args.IsService())
            {
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                Directory.SetCurrentDirectory(pathToContentRoot);
            }

            return args;
        }

        public static void RunAdaptive(this IWebHost host, string[] args)
        {
            if (args.IsService())
            {
                host.RunAsService();
            }
            else
            {
                host.Run();
            }
        }

        private static bool IsService(this string[] args)
        {
            return !Debugger.IsAttached && !args.Contains("--console") && RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }
    }
}
