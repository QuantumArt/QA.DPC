using System;
using System.Collections.Generic;
using System.Linq;
using QA.Core.DPC.Loader.Services;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.Actions.Actions.Abstract
{
    public abstract class MarketingProductAction : ActionTaskBase
    {
        #region Constants
        private const string PRODUCT_ERROR_MESSAGE = "ошибка сервера";
        private const string ACTION_ERROR_MESSAGE = "Can't process action";
        #endregion

        #region Private fields
        private readonly IArticleService _articleService;
        private readonly ISettingsService _settingsService;
        private readonly Func<string, IAction> _getService;

        #endregion

        #region Constructor

        protected MarketingProductAction(Func<string, IAction> getService, IArticleService articleService, ISettingsService settingsService)
        {
            _articleService = articleService;
            _settingsService = settingsService;
            _getService = getService;
        }
        #endregion
       
        protected abstract string ActionKey { get; }

        #region Overrides
        public override string Process(ActionContext context)
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

            var marketingProducts = _articleService.List(marketingProductContentId, context.ContentItemIds).ToArray();
            string productsFieldName = _settingsService.GetSetting(SettingsTitles.MARKETING_PRODUCT_PRODUCTS_FIELD_NAME);
            var productsMap = marketingProducts.ToDictionary(p => p.Id, p => p.FieldValues.Where(fv => fv.Field.Name == productsFieldName).SelectMany(fv => fv.RelatedItems).ToArray());
            int count = productsMap.Count + productsMap.Values.Sum(products => products.Length);
            var exceptions = new List<ProductException>();
            int index = 0;

            IAction action = _getService(ActionKey);

            foreach (var marketingProduct in marketingProducts)
            {
                int[] productIds = productsMap[marketingProduct.Id];
                int remainingCount = productIds.Length;

                foreach (int productId in productIds)
                {
                    if (TaskContext.IsCancellationRequested)
                    {
                        TaskContext.IsCancelled = true;
                        break;
                    }

                    var productContext = GetContext(context, productContentId, productId);

                    try
                    {
                        action.Process(productContext);
                        remainingCount--;
                    }
                    catch (ProductException pex)
                    {
                        exceptions.Add(pex);
                        count -= remainingCount;
                        break;
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(new ProductException(productId, PRODUCT_ERROR_MESSAGE, ex));
                        count -= remainingCount;
                        break;
                    }

                    SetProgress(++index, count);
                }

                if (TaskContext.IsCancellationRequested)
                {
                    TaskContext.IsCancelled = true;
                    break;
                }

                if (remainingCount == 0)
                {
                    var marketingProductContext = GetContext(context, marketingProduct.ContentId, marketingProduct.Id);

                    try
                    {
                        action.Process(marketingProductContext);
                    }
                    catch (ProductException pex)
                    {
                        exceptions.Add(pex);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(new ProductException(marketingProduct.Id, PRODUCT_ERROR_MESSAGE, ex));
                    }

                    SetProgress(++index, count);
                }
            }

            if (exceptions.Any())
            {
                throw new ActionException(ACTION_ERROR_MESSAGE, exceptions, context);
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region Private methods
        private void SetProgress(int index, int count)
        {
            byte progress = (byte)(index * 100 / count);
            TaskContext.SetProgress(progress);
        }

        private ActionContext GetContext(ActionContext context, int contentId, int id)
        {
            return new ActionContext
            {
                BackendSid = context.BackendSid,
                CustomerCode = context.CustomerCode,
                ContentId = contentId,
                ContentItemIds = new[] { id },
                ActionCode = context.ActionCode,
                Parameters = context.Parameters,
                UserId = context.UserId,
                UserName = context.UserName
            };
        }
        #endregion
    }
}
