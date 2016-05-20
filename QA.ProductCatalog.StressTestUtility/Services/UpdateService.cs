using QA.Core;
using QA.Core.ProductCatalog.Actions.Services;
using Quantumart.QP8.BLL;
using System;
using System.Linq;
using System.Threading;
using QA.Core.DPC.Loader.Services;

namespace QA.ProductCatalog.StressTestUtility.Services
{
	public class UpdateService : IUpdateService
	{
		private const string FieldNameForUpdate = "Title";
		private const string ErrorMessage = "Updating article {0} error";

		private readonly IArticleService _articleService;
		private readonly Func<ITransaction> _createTransaction;
		private readonly ILogger _logger;

		public UpdateService(IArticleService articleService, Func<ITransaction> createTransaction, ILogger logger)
		{
			_articleService = articleService;
			_createTransaction = createTransaction;
			_logger = logger;
		}

		public void Update(int articleId)
		{
			try
			{
				using (var transaction = _createTransaction())
				{
					_logger.Info("Start update " + articleId + " Thread = " + Thread.CurrentThread.Name);
					var article = _articleService.Read(articleId);
					Update(article);
					_articleService.Save(article);
					transaction.Commit();
					_logger.Info("End update " + articleId + " Thread = " + Thread.CurrentThread.Name);
				}
			}
			catch (Exception ex)
			{
				_logger.ErrorException(string.Format(ErrorMessage, articleId), ex);
			}
		}

		private void Update(Article article)
		{
			var fieldValue = article.FieldValues.FirstOrDefault(fv => fv.Field.Name == FieldNameForUpdate);

			article.StatusTypeId = Configuration.StatusTypeId;

			if (fieldValue != null)
			{
				var values = fieldValue.Value.Split(new[] { '_' }, 2);
				string baseValue = values[0];
				int iteration = 1;

				if (values.Length > 1)
				{
					if (int.TryParse(values[1], out iteration))
					{
						iteration++;
					}
					else
					{
						iteration = 1;
					}
				}

				fieldValue.UpdateValue(baseValue + "_" + iteration);
			}
		}
	}
}
