using NUnit.Framework;
using QA.Core.DPC.UI;

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
            UnityConfig.Configure();
        }     
    }
}