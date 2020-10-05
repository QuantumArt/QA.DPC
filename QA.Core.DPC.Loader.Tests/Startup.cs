using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using QA.Validation.Xaml.Extensions.Rules;
using Quantumart.QP8.BLL.Services.MultistepActions.Publish;
using System.IO;
using Unity;

namespace QA.Core.DPC.Loader.Tests
{
    [SetUpFixture]
    public class Startup
    {
        public static IUnityContainer Container;

        [OneTimeSetUp]
        public static void StartUp()
        {
            // ReSharper disable once UnusedVariable
            var processRemoteValidationIf = new ProcessRemoteValidationIf { Condition = null };
            TestContext.WriteLine("Started!");
            TestContext.WriteLine("stub");

            string curDir = Directory.GetCurrentDirectory();
            var builder = new ConfigurationBuilder()
                .SetBasePath(curDir)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var configuration = builder.Build();
            var conn = configuration.GetConnectionString("qp_database");

            Container = UnityConfig.Configure(conn);
        }
    }
}