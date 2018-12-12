using System.Collections.Concurrent;
using System.Collections.Generic;

namespace QA.ProductCatalog.Infrastructure
{
    public interface IValidationService
    {
        void UpdateValidationInfo(int[] productIds, ConcurrentDictionary<int, string> errors);
        void ValidateAndUpdate(int[] productIds, Dictionary<int, string> errors);
        int[] GetProductIds();        
    }
}
