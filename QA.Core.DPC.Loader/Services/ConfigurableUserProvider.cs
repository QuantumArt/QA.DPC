using Microsoft.Extensions.Options;
using QA.Core.DPC.QP.Models;
using QA.ProductCatalog.ContentProviders;

namespace QA.Core.ProductCatalog.Actions.Services
{
	public class ConfigurableUserProvider : IUserProvider
	{
		private AuthProperties _properties;

		public ConfigurableUserProvider(IOptions<AuthProperties> properties)
		{
			_properties = properties.Value;
		}

		public int GetUserId()
		{
			return _properties.UserId;
		}

		public string GetUserName()
		{
			return "Configured user";
		}

		public int GetLanguageId()
		{
			return 1;
		}
	}
}