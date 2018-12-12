using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.ProductCatalog.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace QA.Core.ProductCatalog.Actions.Actions
{
    public class ValidationAction : ActionTaskBase
    {
        private const int UpdateChunkSize = 1000;
        private readonly IValidationService _validationService;        

        public ValidationAction(IValidationService validationService)
        {
            _validationService = validationService;
        }

        public override string Process(ActionContext context)
        {
            var errors = new Dictionary<int, string>();
            var productIds = _validationService.GetProductIds();
            int page = 0;

            foreach (var batch in productIds.Section(UpdateChunkSize))
            {
                if (TaskContext.IsCancellationRequested)
                {
                    TaskContext.IsCancelled = true;
                    break;
                }

                _validationService.ValidateAndUpdate(batch.ToArray(), errors);

                byte progress = (byte)(++page * UpdateChunkSize * 100 / productIds.Length);
            }

            return $"{productIds.Length} products are validated; {errors.Keys.Count} products are invalid";
        }
    }
}
