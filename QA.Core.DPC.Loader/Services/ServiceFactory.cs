using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL.Services.API;
using System;
using QA.Core.DPC.QP.Services;

namespace QA.Core.ProductCatalog.Actions.Services
{
    public class ServiceFactory : IServiceFactory
	{
		private readonly string _connectionString;
		private readonly IUserProvider _userProvider;

		public ServiceFactory(IConnectionProvider connectionProvider, IUserProvider userProvider)
		{
			//TODO: Use code contracts instead of code below

			if (connectionProvider == null)
				throw new ArgumentNullException("connectionProvider");

			if (userProvider == null)
				throw new ArgumentNullException("userProvider");
		

			_connectionString = connectionProvider.GetConnection();
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