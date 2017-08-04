using System.Linq;
using QA.ProductCatalog.Infrastructure;
using QA.Validation.Xaml.Extensions.Rules;

namespace QA.ProductCatalog.Validation.Validators
{
	public class RemoteValidatorDecorator : IRemoteValidator2
	{
		private readonly IRemoteValidator2[] _validators;

		public RemoteValidatorDecorator(IRemoteValidator2[] validators)
		{
			_validators = validators;
		}

		public RemoteValidationResult Validate(RemoteValidationContext context, RemoteValidationResult result)
		{
		    return _validators.Aggregate(result, (current, validator) => validator.Validate(context, current));
		}
	}
}
