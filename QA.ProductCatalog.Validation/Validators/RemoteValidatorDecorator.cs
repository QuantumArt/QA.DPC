using QA.ProductCatalog.Infrastructure;
using QA.Validation.Xaml;
using QA.Validation.Xaml.Extensions.Rules;

namespace QA.ProductCatalog.Validation.Validators
{
	public class RemoteValidatorDecorator : IRemoteValidator
	{
		private readonly IRemoteValidator[] _validators;

		public RemoteValidatorDecorator(IRemoteValidator[] validators)
		{
			_validators = validators;
		}

		public void Validate(RemoteValidationContext context, ref ValidationContext result)
		{
			foreach (var validator in _validators)
			{
				validator.Validate(context, ref result);
			}
		}
	}
}
