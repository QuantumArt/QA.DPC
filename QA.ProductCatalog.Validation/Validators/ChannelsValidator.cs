using System.Linq;
using QA.Core.Models.Processors;
using QA.ProductCatalog.Infrastructure;
using QA.Validation.Xaml;
using QA.Validation.Xaml.Extensions.Rules;

namespace QA.ProductCatalog.Validation.Validators
{
    public class ChannelsValidator : IRemoteValidator
    {
        private const string FieldFilter = "Filter";

        public void Validate(RemoteValidationContext context, ref ValidationContext result)
        {
            var filterDefinition = context.Definitions.FirstOrDefault(x => x.Alias == FieldFilter);
            if (filterDefinition == null)
            {
                result.AddErrorMessage("Не найдено поле " + FieldFilter);
            }

            var filter = context.ProvideValueExact<string>(filterDefinition);
            if (!string.IsNullOrEmpty(filter))
            {
                if (!DPathProcessor.IsExpressionValid(filter))
                {
                    result.AddModelError(filterDefinition.PropertyName, "Невалидный фильтр");
                }
            }
        }
    }
}
