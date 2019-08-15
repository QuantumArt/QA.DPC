using QA.Core.DPC.QP.Models;
using QA.ProductCatalog.ContentProviders;

namespace QA.Core.ProductCatalog.Actions.Services
{
	public class ConfigurableUserProvider : IUserProvider
	{
		private AuthProperties _properties;

		public ConfigurableUserProvider(AuthProperties properties)
		{
			_properties = properties;
		}

		public int GetUserId()
		{
			return _properties.UserId;
		}

		public string GetUserName()
		{
			return "Configured user";
		}
	}
}