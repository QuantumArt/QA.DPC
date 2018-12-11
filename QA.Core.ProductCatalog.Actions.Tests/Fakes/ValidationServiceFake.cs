using QA.ProductCatalog.Infrastructure;
using System.Collections.Concurrent;

namespace QA.Core.ProductCatalog.Actions.Tests.Fakes
{
    public class ValidationServiceFake : IValidationService
    {
        public void UpdateValidationInfo(int[] productIds, ConcurrentDictionary<int, string> errors)
        {
        }
    }
}