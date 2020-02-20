using QA.Core;
using QA.Core.ProductCatalog.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;
using ActionContext = QA.Core.ProductCatalog.Actions.ActionContext;

namespace QA.ProductCatalog.Admin.WebApp.Binders
{
    public class ActionContextModelBinder : IModelBinder
	{
		#region Constants
		private const string CustomerCodeKey = "customerCode";
		private const string BackendSidKey = "backend_sid";
		private const string ContentIdKey = "content_id";
		private const string ContentItemIdKey = "content_item_id";
		private const string ActionCodeKey = "actioncode";
		#endregion

        private readonly IUserProvider _provider;
		
        public ActionContextModelBinder(IUserProvider provider)
        {
            _provider = provider;
        }

		private string GetValue(string key, ModelBindingContext bindingContext)
		{
			var value = bindingContext.ValueProvider.GetValue(key);
			return value.FirstValue;
		}

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var context = new ActionContext
            {
                Parameters =
                    bindingContext.HttpContext.Request.Form.Keys
                        .ToDictionary(x => x, x => bindingContext.HttpContext.Request.Form[x].ToString()),
                CustomerCode = GetValue(CustomerCodeKey, bindingContext)
            };

            Guid.TryParse(GetValue(BackendSidKey, bindingContext), out var backendSid);

            context.BackendSid = backendSid;
            context.UserId = _provider.GetUserId();
            context.UserName = _provider.GetUserName();

            int.TryParse(GetValue(ContentIdKey, bindingContext), out var contentId);
            
            context.ContentId = contentId;
            context.ActionCode = GetValue(ActionCodeKey, bindingContext);

            var contentItems = GetValue(ContentItemIdKey, bindingContext);
            var contentItemIds = new List<int>();

            if (!string.IsNullOrEmpty(contentItems))
            {
                var ids = contentItems.Split(',');

                foreach (var id in ids)
                {
                    if (int.TryParse(id, out var contentItemId))
                    {
                        if (contentItemId <= 0)
                        {
                            bindingContext.ModelState.AddModelError(ContentItemIdKey, id + " must be greater than 0");
                            return Task.CompletedTask;
                        }

                        contentItemIds.Add(contentItemId);
                    }
                    else
                    {
                        bindingContext.ModelState.AddModelError(ContentItemIdKey, id + " must be integer");
                        return Task.CompletedTask;
                    }
                }
            }

            context.ContentItemIds = contentItemIds.ToArray();

            bindingContext.Result = ModelBindingResult.Success(context);
            return Task.CompletedTask;
        }
    }
    
    public class ActionContextModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(ActionContext))
            {
                return new BinderTypeModelBinder(typeof(ActionContextModelBinder));
            }

            return null;
        }
    }
}