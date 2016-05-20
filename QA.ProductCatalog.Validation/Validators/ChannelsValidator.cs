using System.Linq;
using QA.Core.Models.UI;
using QA.ProductCatalog.Infrastructure;
using QA.Validation.Xaml;
using QA.Validation.Xaml.Extensions.Rules;

namespace QA.ProductCatalog.Validation.Validators
{
	public class ChannelsValidator : IRemoteValidator
	{
		private const string FIELD_FILTER = "Filter";

		public void Validate(RemoteValidationContext context, ref ValidationContext result)
		{
			var filterDefinition = context.Definitions.FirstOrDefault(x => x.Alias == FIELD_FILTER);

			if (filterDefinition == null)
			{
				result.AddErrorMessage("Не найдено поле " + FIELD_FILTER);
			}

			string filter = context.ProvideValueExact<string>(filterDefinition);

			if (!string.IsNullOrEmpty(filter))
			{
				var match = FilterableBindingValueProvider.VirtualFieldPathRegex.Match(filter);

				if (!match.Success)
				{
					result.AddModelError(filterDefinition.PropertyName, "Невалидный фильтр");
				}
			}
		}
	}
}
