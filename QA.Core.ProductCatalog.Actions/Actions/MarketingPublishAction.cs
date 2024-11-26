using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using System;
using System.Linq;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.Resources;
using NLog;
using NLog.Fluent;
using QA.ProductCatalog.ContentProviders;

namespace QA.Core.ProductCatalog.Actions.Actions
{
    public class MarketingPublishAction : ActionTaskBase
	{
		#region Constants
		private const string PublishActionKey = "PublishAction";
		private const string IgnoredStatusKey = "IgnoredStatus";
		private const string AdapterKey = "Adapter";
		private const string LoggerErrorMessage = "Can't publish marketing products: ";
		#endregion

		#region Private properties
		private readonly  IArticleService _articleService;
		private readonly  IFieldService _fieldService;
		private readonly ISettingsService _settingsService;
		#endregion

		#region Constructor
		public MarketingPublishAction(IArticleService articleService, IFieldService fieldService, ISettingsService settingsService)
		{
			_articleService = articleService;
			_fieldService = fieldService;
			_settingsService = settingsService;
		}
		#endregion

		#region IAction implementation
		public override ActionTaskResult Process(ActionContext context)
		{
			if (context == null)
				throw new ArgumentNullException("context");

			if (context.ContentItemIds == null || context.ContentItemIds.Length == 0)
				throw new ArgumentException("ContentItemIds cant be empty", "context.ContentItemIds");

			try
			{
				string adapter = GetAdapter(context);
				var marketingProducts = _articleService.List(context.ContentId, context.ContentItemIds).ToArray();
				string ignoredStatus = GetIgnoredStatus(context);
				string[] ignoredStatuses = (ignoredStatus == null) ? Enumerable.Empty<string>().ToArray() : ignoredStatus.Split(new[] { ',' });

                string productsFieldName = _settingsService.GetSetting(SettingsTitles.MARKETING_PRODUCT_PRODUCTS_FIELD_NAME);

				int backRelationFieldId = marketingProducts[0].FieldValues.Single(fv => fv.Field.Name == productsFieldName).Field.BackRelationId.Value;
				var backRelationField = _fieldService.Read(backRelationFieldId);
				int productContentId = backRelationField.ContentId;

			    int[] productIds = DoWithLogging(
				    () => Helpers.GetProductIdsFromMarketingProducts(context.ContentItemIds, _articleService, _settingsService),
				    "Receiving regional products from marketing product ids {ids} ", context.ContentItemIds
				);

				var filteredProductIds = _articleService.List(productContentId, productIds)
										.Where(a => !ignoredStatuses.Contains(a.Status.Name) && !a.Archived && a.Visible)
										.Select(a => a.Id)
										.ToArray();

				ActionTaskResult result;

				if (filteredProductIds.Any())
				{
					
					var publishAction = ObjectFactoryBase.Resolve<PublishAction>();

					var productContext = new ActionContext
					{
						BackendSid = context.BackendSid,
						CustomerCode = context.CustomerCode,
						ContentId = productContentId,
						ContentItemIds = filteredProductIds,
						ActionCode = context.ActionCode,
						Parameters = context.Parameters,
                        UserId = context.UserId,
                        UserName = context.UserName
					};
					
					
					result = DoWithLogging(
						() => publishAction.Process(productContext),
						"Calling Publish Action for products {ids}",
						filteredProductIds
					);
				}
				else
				{
					result = ActionTaskResult.Error(new ActionTaskResultMessage()
					{
						ResourceClass = nameof(TaskStrings),
						ResourceName = nameof(TaskStrings.NoProductsToPublish)
					}, context.ContentItemIds);      
				}

				var excludedProductIds = productIds.Except(filteredProductIds).ToArray();

				if (excludedProductIds.Any())
				{
					result.Messages.Add(new ActionTaskResultMessage()
					{
						ResourceClass = nameof(TaskStrings),
						ResourceName = nameof(TaskStrings.FilteredProducts),
						Parameters = new [] {string.Join(", ", excludedProductIds), ignoredStatus}
					});
				}

				return result;
			}
			catch (ActionException)
			{
				throw;
			}
			catch (Exception ex) {
				Logger.ForErrorEvent().Message(LoggerErrorMessage).Exception(ex).Log();
				throw new ActionException(
					TaskStrings.ActionErrorMessage, 
					context.ContentItemIds.Select(
						id => new ProductException(id, nameof(TaskStrings.ServerError), ex)
					),
					context
				);
			}			
		}

	    #endregion

		#region Private mehtods
		

		private static string GetIgnoredStatus(ActionContext context)
		{
			if (context.Parameters.ContainsKey(IgnoredStatusKey))
			{
				return context.Parameters[IgnoredStatusKey];
			}
			else
			{
				return null;
			}
		}

		private static string GetAdapter(ActionContext context)
		{
			if (context.Parameters.ContainsKey(AdapterKey))
			{
				return context.Parameters[AdapterKey];
			}
			else
			{
				return null;
			}
		}

		#endregion
	}
}