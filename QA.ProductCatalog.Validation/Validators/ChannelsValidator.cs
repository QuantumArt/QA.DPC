using System.Linq;
using QA.Core.Models.Processors;
using QA.ProductCatalog.Infrastructure;
using QA.Validation.Xaml;
using QA.Validation.Xaml.Extensions.Rules;

namespace QA.ProductCatalog.Validation.Validators
{
    public class ChannelsValidator : IRemoteValidator2
    {
        private const string FieldFilter = "Filter";

        public RemoteValidationResult Validate(RemoteValidationContext context, RemoteValidationResult result)
        {
            var filterDefinition = context.Definitions.FirstOrDefault(x => x.Alias == FieldFilter);
            if (filterDefinition == null)
            {
                result.AddErrorMessage("Не найдено поле " + FieldFilter);
            }
            else
            {
                var filter = context.ProvideValueExact<string>(filterDefinition);
                if (string.IsNullOrEmpty(filter)) return result;
                var normalizedFilter = DPathProcessor.NormalizeExpression(filter);
                if (!DPathProcessor.IsExpressionValid(normalizedFilter))
                {
                    result.AddModelError(filterDefinition.PropertyName, $"Невалидный фильтр: {normalizedFilter}");
                }
            }


            return result;
        }
    }
}
