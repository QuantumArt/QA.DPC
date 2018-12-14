using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.Actions.Actions
{
    public class ValidationAction : ActionTaskBase
    {
        private const int DefaultChunkSize = 1000;
        private const int DefaultMaxDegreeOfParallelism = 1;
        private readonly IValidationService _validationService;        

        public ValidationAction(IValidationService validationService)
        {
            _validationService = validationService;
        }

        public override string Process(ActionContext context)
        {
            int chunkSize = GetValue(context, "UpdateChunkSize", DefaultChunkSize);
            int maxDegreeOfParallelism = GetValue(context, "MaxDegreeOfParallelism", DefaultMaxDegreeOfParallelism);
            
            var report =_validationService.ValidateAndUpdate(chunkSize, maxDegreeOfParallelism, TaskContext);
            return $"Products: {report.TotalProductsCount};" +
                    $"Updated products: {report.UpdatedProductsCount};" +
                    $"Validated products: {report.ValidatedProductsCount};" +
                    $"Invalid products: {report.InvalidProductsCount};" +
                    $"Validation errors: {report.ValidationErrorsCount}";           
        }

        private int GetValue(ActionContext context, string key, int defaultValue)
        {
            int value = DefaultChunkSize;

            if (context.Parameters.TryGetValue(key, out string setting))
            {
                int.TryParse(setting, out value);
            }

            return value;
        }
    }
}
