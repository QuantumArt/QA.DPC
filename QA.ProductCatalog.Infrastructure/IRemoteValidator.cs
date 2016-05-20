using QA.Validation.Xaml;
using QA.Validation.Xaml.Extensions.Rules;

namespace QA.ProductCatalog.Infrastructure
{
	public interface IRemoteValidator
	{
		void Validate(RemoteValidationContext context, ref ValidationContext result);
	}
}