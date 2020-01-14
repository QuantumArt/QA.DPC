using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.Actions.Actions
{
    public class ValidationAction : ActionTaskBase
    {
        private static readonly object Locker = new object();
        private static bool IsProcessing = false;
        private const int DefaultChunkSize = 1000;
        private const int DefaultMaxDegreeOfParallelism = 1;
        private readonly IValidationService _validationService;

        public ValidationAction(IValidationService validationService)
        {
            _validationService = validationService;
        }

        public override ActionTaskResult Process(ActionContext context)
        {
            bool canProcess = false;
            lock (Locker)
            {
                if (!IsProcessing)
                {
                    IsProcessing = true;
                    canProcess = true;
                }
            }

            if (canProcess)
            {
                try
                {
                    int chunkSize = GetValue(context, "UpdateChunkSize", DefaultChunkSize);
                    int maxDegreeOfParallelism = GetValue(context, "MaxDegreeOfParallelism", DefaultMaxDegreeOfParallelism);

                    var report = _validationService.ValidateAndUpdate(chunkSize, maxDegreeOfParallelism, TaskContext);                    
                    return ActionTaskResult.Success($"Products: {report.TotalProductsCount};" +
                            $"Updated products: {report.UpdatedProductsCount};" +
                            $"Validated products: {report.ValidatedProductsCount};" +
                            $"Invalid products: {report.InvalidProductsCount};" +
                            $"Validation errors: {report.ValidationErrorsCount}");
                }
                finally
                {
                    IsProcessing = false;
                }
            }
            else
            {
                return ActionTaskResult.Error("ValidationAction is already running");
            }
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
