using QA.Core;
using QA.Core.ProductCatalog.Actions.Services;
using System;
using System.Threading;
using QA.Core.DPC.Loader.Services;

namespace QA.ProductCatalog.StressTestUtility.Services
{
	public class SimplePublishService : ISimplePublishService
	{
		private const string ErrorMessage = "Simple publish error for articles {0}";

		private readonly IArticleService _articleService;
		private readonly Func<ITransaction> _createTransaction;
		private readonly ILogger _logger;

		public SimplePublishService(IArticleService articleService, Func<ITransaction> createTransaction, ILogger logger)
		{
			_articleService = articleService;
			_createTransaction = createTransaction;
			_logger = logger;
		}

		public void Publish(int[] ids)
		{
			string idsDescription = string.Join(",", ids);

			try
			{
				using (var transaction = _createTransaction())
				{
					_logger.Info("Start simple publish " + idsDescription + " Thread = " + Thread.CurrentThread.Name);
					_articleService.SimplePublish(ids);
					transaction.Commit();
					_logger.Info("End simple publish " + idsDescription + " Thread = " + Thread.CurrentThread.Name);
				}
			}
			catch (Exception ex)
			{
				_logger.ErrorException(string.Format(ErrorMessage, idsDescription), ex);
			}
		}
	}
}
