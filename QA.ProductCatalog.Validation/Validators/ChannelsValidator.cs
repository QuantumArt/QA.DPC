using System.Linq;
using Newtonsoft.Json;
using QA.Core.DPC.Resources;
using QA.Core.Models.Processors;
using QA.ProductCatalog.ContentProviders;
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
                var message = new ActionTaskResultMessage()
                {
                    ResourceClass = ValidationHelper.ResourceClass,
                    ResourceName = nameof(RemoteValidationMessages.MissingParam),
                    Parameters = new object[] {FieldFilter}
                };
                result.AddErrorMessage(JsonConvert.SerializeObject(message));
            }
            else
            {
                var filter = context.ProvideValueExact<string>(filterDefinition);
                if (string.IsNullOrEmpty(filter)) return result;
                var normalizedFilter = DPathProcessor.NormalizeExpression(filter);
                if (!DPathProcessor.IsExpressionValid(normalizedFilter))
                {
                    var message = new ActionTaskResultMessage()
                    {
                        ResourceClass = ValidationHelper.ResourceClass,
                        ResourceName = nameof(RemoteValidationMessages.InvalidFilter),
                        Parameters = new object[] {normalizedFilter}
                    };
                    result.AddModelError(filterDefinition.PropertyName, JsonConvert.SerializeObject(message));
                }
            }


            return result;
        }
    }
}
