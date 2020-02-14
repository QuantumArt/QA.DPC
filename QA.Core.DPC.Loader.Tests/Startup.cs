using NUnit.Framework;
using QA.Validation.Xaml.Extensions.Rules;
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
            Container = UnityConfig.Configure();
        }
    }
}