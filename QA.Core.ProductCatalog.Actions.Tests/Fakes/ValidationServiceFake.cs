using QA.ProductCatalog.Infrastructure;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace QA.Core.ProductCatalog.Actions.Tests.Fakes
{
    public class ValidationServiceFake : IValidationService
    {
        public int[] GetProductIds()
        {
            return new int[0];
        }

        public void UpdateValidationInfo(int[] productIds, ConcurrentDictionary<int, string> errors)
        {
        }

        public void ValidateAndUpdate(int[] productIds, Dictionary<int, string> errors)
        {
        }
    }
}