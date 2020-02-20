using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL.Services.API;
using System;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.ContentProviders;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;

namespace QA.Core.ProductCatalog.Actions.Services
{
    public class ServiceFactory : IServiceFactory
	{
		private readonly Customer _customer;
		private readonly IUserProvider _userProvider;

		public ServiceFactory(IConnectionProvider connectionProvider, IUserProvider userProvider)
		{
			//TODO: Use code contracts instead of code below

			if (connectionProvider == null)
				throw new ArgumentNullException("connectionProvider");

			if (userProvider == null)
				throw new ArgumentNullException("userProvider");
		

			_customer = connectionProvider.GetCustomer();
			_userProvider = userProvider;
		}

		#region IServiceFactory implementation
		public FieldService GetFieldService()
		{
			return new FieldService(new QpConnectionInfo(_customer.ConnectionString, (DatabaseType)_customer.DatabaseType), _userProvider.GetUserId());
		}

		public ArticleService GetArticleService()
		{
			return new ArticleService(new QpConnectionInfo(_customer.ConnectionString, (DatabaseType)_customer.DatabaseType), _userProvider.GetUserId());
		}

		public ContentService GetContentService()
		{
			return new ContentService(new QpConnectionInfo(_customer.ConnectionString, (DatabaseType)_customer.DatabaseType), _userProvider.GetUserId());
		}
		#endregion	
	}
}