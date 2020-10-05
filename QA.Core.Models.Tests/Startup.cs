using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using QA.Core.DPC.UI;
using System.IO;

namespace QA.Core.Models.Tests
{
    [SetUpFixture]
    public class Startup
    {
        [OneTimeSetUp]
        public static void StartUp()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new StackPanel { Name = "" };
            TestContext.WriteLine("Started!");

            string curDir = Directory.GetCurrentDirectory();
            var builder = new ConfigurationBuilder()
                .SetBasePath(curDir)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var configuration = builder.Build();
            var conn = configuration.GetConnectionString("qp_database");

            UnityConfig.Configure();
        }     
    }
}