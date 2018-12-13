using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.Actions.Actions
{
    public class ValidationAction : ActionTaskBase
    {
        private const int DefaultUpdateChunkSize = 100;
        private readonly IValidationService _validationService;        

        public ValidationAction(IValidationService validationService)
        {
            _validationService = validationService;
        }

        public override string Process(ActionContext context)
        {
            int updateChunkSize = DefaultUpdateChunkSize;

            if (context.Parameters.TryGetValue("UpdateChunkSize", out string value))
            {
                int.TryParse(value, out updateChunkSize);
            }

             var report =_validationService.ValidateAndUpdate(updateChunkSize, TaskContext);
            return $"Products: {report.TotalProductsCount}; Updated products: {report.UpdatedProductsCount}; Validated products: {report.ValidatedProductsCount}; Invalid products: {report.InvalidProductsCount}, Validation errors: {report.ValidationErrorsCount}";           
        }
    }
}
