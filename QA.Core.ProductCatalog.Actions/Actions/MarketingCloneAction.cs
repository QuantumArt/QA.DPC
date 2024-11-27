using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.Resources;
using NLog;
using NLog.Fluent;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.ContentProviders;

namespace QA.Core.ProductCatalog.Actions.Actions
{
	public class MarketingCloneAction : ActionTaskBase
	{
		#region Constants
		private const string MarketingMapKey = "MarketingMap";
		private const string LoggerMarketingErrorMessage = "Can't clone marketing product ";
		private const string LoggerErrorMessage = "Can't clone product ";
		private const string FieldIdParameterKey = "FieldId";
		private const string ArticleIdParameterKey = "ArticleId";
		#endregion

		#region Private fields
		private readonly CloneBatchAction _cloneService;
		private readonly IArticleService _articleService;
		private readonly ISettingsService _settingsService;
		private readonly IFieldService _fieldService;
		#endregion

		#region Constructor
		public MarketingCloneAction(CloneBatchAction cloneService, IArticleService articleService, ISettingsService settingsService, IFieldService fieldService)
		{
			_cloneService = cloneService;
			_articleService = articleService;
			_settingsService = settingsService;
			_fieldService = fieldService;
		}
		#endregion

		#region Overrides
		public override ActionTaskResult Process(ActionContext context)
		{
			if (context == null)
				throw new ArgumentNullException("context");

			if (context.ContentItemIds == null || context.ContentItemIds.Length == 0)
				throw new ArgumentException("ContentItemIds cant be empty", "context.ContentItemIds");

			int marketingProductContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.MARKETING_PRODUCT_CONTENT_ID));
			int productContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.PRODUCTS_CONTENT_ID));
		

			if (context.ContentId != marketingProductContentId)
			{
				throw new ArgumentException("Action is available for marketing products only", "context.ContentId");
			}


			var exceptions = new List<ProductException>();
			int index = 0;
			var marketingProducts = _articleService.List(marketingProductContentId, context.ContentItemIds).ToArray();			
			string productsFieldName = _settingsService.GetSetting(SettingsTitles.MARKETING_PRODUCT_PRODUCTS_FIELD_NAME);
			int relationFieldId = marketingProducts[0].FieldValues.Single(fv => fv.Field.Name == productsFieldName).Field.Id;
			int backRelationFieldId = marketingProducts[0].FieldValues.Single(fv => fv.Field.Name == productsFieldName).Field.BackRelationId.Value;
			var productsMap = marketingProducts.ToDictionary(p => p.Id, p => p.FieldValues.Where(fv => fv.Field.Name == productsFieldName).SelectMany(fv => fv.RelatedItems).ToArray());
			int count = productsMap.Count() + productsMap.Values.Sum(products => products.Count());

			foreach (var marketingProduct in marketingProducts)
			{
				if (TaskContext.IsCancellationRequested)
				{
					TaskContext.IsCancelled = true;
					break;
				}

				int marketingProductCloneId = DoWithLogging(
					() => CloneMarketingProduct(context, marketingProduct, relationFieldId, exceptions),
					"Cloning marketing product {id}", marketingProduct.Id
				);
				
				SetProgress(++index, count);

				if (marketingProductCloneId == 0)
				{
					index += productsMap[marketingProduct.Id].Count();
					SetProgress(index, count);
				}
				else
				{
					foreach (int productId in productsMap[marketingProduct.Id])
					{
						if (TaskContext.IsCancellationRequested)
						{
							TaskContext.IsCancelled = true;
							break;
						}

						DoWithLogging(
							() => CloneProduct(context, productId, productContentId, marketingProductCloneId, backRelationFieldId, exceptions),
							"Cloning regional product {id} into marketing product {marketingProductId} ", productId, marketingProductCloneId
						);
						SetProgress(++index, count);
					}
				}
			}

			if (exceptions.Any())
			{
				throw new ActionException(TaskStrings.ActionErrorMessage, exceptions, context);
			}
			else
			{
				return null;
			}
			
		}
		#endregion

		#region Private methods
		private int CloneMarketingProduct(ActionContext context, Article marketingProduct, int relationFieldId, List<ProductException> exceptions)
		{
			try
			{
				var parameters = GetMarketingParameters(relationFieldId);
				var marketingContext = GetContext(context, marketingProduct.ContentId, marketingProduct.Id, parameters);
				_cloneService.Process(marketingContext);
				int id = _cloneService.Ids.First();
				return id;			
			}
			catch (ProductException pex)
			{
				var logLevel = pex.IsError ? LogLevel.Error : LogLevel.Info;
				Logger.ForLogEvent(logLevel).Message(LoggerMarketingErrorMessage + marketingProduct.Id).Exception(pex).Log();
				exceptions.Add(pex);
				return 0;
			}
			catch (Exception ex)
			{
				Logger.ForErrorEvent().Message(LoggerMarketingErrorMessage + marketingProduct.Id).Exception(ex).Log();
				exceptions.Add(new ProductException(marketingProduct.Id, nameof(TaskStrings.ServerError), ex) { IsError = true });
				return 0;
			}
		}

		private void CloneProduct(ActionContext context, int productId, int productContentId, int marketingProductCloneId, int backRelationFieldId, List<ProductException> exceptions)
		{
			try
			{
				var parameters = GetParameters(marketingProductCloneId, backRelationFieldId);
				var regionalContext = GetContext(context, productContentId, productId, parameters);
				_cloneService.Process(regionalContext);
			}
			catch (ProductException pex)
			{
				var logLevel = pex.IsError ? LogLevel.Error : LogLevel.Info;
				Logger.ForLogEvent(logLevel).Message(LoggerErrorMessage + productId).Exception(pex).Log();
				exceptions.Add(pex);
			}
			catch (Exception ex)
			{
				Logger.ForErrorEvent().Message(LoggerErrorMessage + productId).Exception(ex).Log();
				exceptions.Add(new ProductException(productId, nameof(TaskStrings.ServerError), ex) { IsError = true });
			}
		}

		private void SetProgress(int index, int count)
		{
			byte progress = (byte)(index * 100 / count);
			TaskContext.SetProgress(progress);
		}

		private Dictionary<string, string> GetMarketingParameters(int relationFieldId)
		{
			return new Dictionary<string, string>()
			{
				{ FieldIdParameterKey, relationFieldId.ToString()} ,
			};
		}

		private Dictionary<string, string> GetParameters(int marketingProductCloneId, int backRelationFieldId)
		{
			return new Dictionary<string, string>
			{
				{ ArticleIdParameterKey, marketingProductCloneId.ToString() },
				{ FieldIdParameterKey, backRelationFieldId.ToString() },
			};
		}

		private ActionContext GetContext(ActionContext context, int contentId, int id, Dictionary<string,string> parameters)
		{
			return new ActionContext
			{
				BackendSid = context.BackendSid,
				CustomerCode = context.CustomerCode,
				ContentId = contentId,
				ContentItemIds = new []{ id },
				ActionCode = context.ActionCode,
				Parameters = parameters,
				UserId = context.UserId,
				UserName = context.UserName
			};
		}
		#endregion
	}
}
