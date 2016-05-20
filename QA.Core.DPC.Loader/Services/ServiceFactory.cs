using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QPublishing;
using Quantumart.QPublishing.Database;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.ProductCatalog.Actions.Services
{
	public class ServiceFactory : IServiceFactory
	{
		private readonly string _connectionString;
		private readonly IUserProvider _userProvider;

		public ServiceFactory(string connectionString, IUserProvider userProvider)
		{
			//TODO: Use code contracts instead of code below

			if (connectionString == null)
				throw new ArgumentNullException("connectionString");

			if (userProvider == null)
				throw new ArgumentNullException("userProvider");

			if (connectionString == "")
				throw new ArgumentException("connection string is empty", "connectionString");

			_connectionString = connectionString;
			_userProvider = userProvider;
		}

		#region IServiceFactory implementation
		public FieldService GetFieldService()
		{
			return new FieldService(_connectionString, _userProvider.GetUserId());
		}

		public ArticleService GetArticleService()
		{
			return new ArticleService(_connectionString, _userProvider.GetUserId());
		}

		public ContentService GetContentService()
		{
			return new ContentService(_connectionString, _userProvider.GetUserId());
		}
		#endregion	
	}
}