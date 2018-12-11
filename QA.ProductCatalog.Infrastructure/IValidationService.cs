using System.Collections.Concurrent;
using System.Collections.Generic;

namespace QA.ProductCatalog.Infrastructure
{
    public interface IValidationService
    {
        void UpdateValidationInfo(int[] productIds, ConcurrentDictionary<int, string> errors);
    }
}
