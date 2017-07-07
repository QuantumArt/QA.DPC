using System;
using QA.Validation.Xaml;
using QA.Validation.Xaml.Extensions.Rules;

namespace QA.ProductCatalog.Infrastructure
{
    [Obsolete("Use IRemoteValidator2")]
	public interface IRemoteValidator
	{
		void Validate(RemoteValidationContext context, ref ValidationContext result);
	}
}