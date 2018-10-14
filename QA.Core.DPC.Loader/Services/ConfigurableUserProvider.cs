using System;
using System.Configuration;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.Actions.Services
{
	public class ConfigurableUserProvider : IUserProvider
	{
		private const int DefaultUserId = 1;	 

		public int GetUserId()
		{
			return int.TryParse(ConfigurationManager.AppSettings["UserId"], out var userId) ? userId : DefaultUserId;
		}

		public string GetUserName()
		{
			return "Configured user";
		}
	}
}