using QA.Core.ProductCatalog.Actions;
using System;
using System.Threading;
using QA.Core.Logger;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;

namespace QA.ProductCatalog.StressTestUtility.Services
{
    public class PublishService : IPublishService
	{
		private const int ContentId = 288;

		private readonly Func<IAction> _actionFactory;
		private readonly ILogger _logger;

		public PublishService(Func<IAction> actionFactory, ILogger logger)
		{
			_actionFactory = actionFactory;
			_logger = logger;
		}

		public void Publish(int productId)
		{			
			var action = _actionFactory();
			var context = GetActionContext(productId);
			try
			{
				_logger.Info("Start publish " + productId + " Thread = " + Thread.CurrentThread.Name);
				action.Process(context);
				_logger.Info("End publish " + productId + " Thread = " + Thread.CurrentThread.Name);
			}
			catch
			{
			}
		}

		private ActionContext GetActionContext(int productId)
		{
            return new ActionContext { ContentId = ContentId, ContentItemIds = new[] { productId } };
		}
	}
}
