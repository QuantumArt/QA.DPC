using System.Collections.Concurrent;

namespace QA.ProductCatalog.Infrastructure
{
    public interface IValidationService
    {
        void UpdateValidationInfo(int[] productIds, ConcurrentDictionary<int, string> errors);
        ValidationReport ValidateAndUpdate(int updateChunkSize, ITaskExecutionContext context);     
    }

    public class ValidationReport
    {
        public int TotalProductsCount { get; set; }
        public int ValidatedProductsCount { get; set; }
        public int UpdatedProductsCount { get; set; }
        public int InvalidProductsCount { get; set; }
        public int ValidationErrorsCount { get; set; }
    }
}