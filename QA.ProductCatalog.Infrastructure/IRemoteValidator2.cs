using QA.Validation.Xaml;
using QA.Validation.Xaml.Extensions.Rules;

namespace QA.ProductCatalog.Infrastructure
{
	public interface IRemoteValidator2
	{
        RemoteValidationResult Validate(RemoteValidationContext context, RemoteValidationResult result);
	}
}