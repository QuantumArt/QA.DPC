using System;
using System.Configuration;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.Actions.Services
{
	public class ConfigurableUserProvider : IUserProvider
	{
		private const int DefaultUserId = 1;	 

		public int GetUserId()
		{
			int userId;

			if (int.TryParse(ConfigurationManager.AppSettings["UserId"], out userId))
			{
				return userId;
			}
			else
			{
				return DefaultUserId;
			}
		}

		public string GetUserName()
		{
			throw new NotImplementedException();
		}
	}
}