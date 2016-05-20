using QA.Core;
using QA.Core.ProductCatalog.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QA.ProductCatalog.Infrastructure;

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

		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			var context = new ActionContext
			{
				Parameters =
					controllerContext.RequestContext.HttpContext.Request.Form.AllKeys
						.ToDictionary(x => x, x => controllerContext.RequestContext.HttpContext.Request.Form[x]),
				CustomerCode = GetValue(CustomerCodeKey, bindingContext)
			};

			Guid backendSid;
			
			Guid.TryParse(GetValue(BackendSidKey, bindingContext), out backendSid);
			
			context.BackendSid = backendSid;

			var userProvider = ObjectFactoryBase.Resolve<IUserProvider>();

			context.UserId = userProvider.GetUserId();

			context.UserName = userProvider.GetUserName();

			int contentId;
			int.TryParse(GetValue(ContentIdKey, bindingContext), out contentId);
			context.ContentId = contentId;

			context.ActionCode = GetValue(ActionCodeKey, bindingContext);

			string contentItems = GetValue(ContentItemIdKey, bindingContext);

		    var contentItemIds = new List<int>();

			if (!string.IsNullOrEmpty(contentItems))
			{
				string[] ids = contentItems.Split(',');

				foreach (string id in ids)
				{
					int contentItemId;
					if (int.TryParse(id, out contentItemId))
					{
						if (contentItemId <= 0)
						{
							bindingContext.ModelState.AddModelError(ContentItemIdKey, id + " must be greater than 0");
						}
                        contentItemIds.Add(contentItemId);
					}
					else
					{
						bindingContext.ModelState.AddModelError(ContentItemIdKey, id + " must be integer");
					}
				}
			}

		    context.ContentItemIds = contentItemIds.ToArray();

			return context;
		}

		private string GetValue(string key, ModelBindingContext bindingContext)
		{
			var value = bindingContext.ValueProvider.GetValue(key);
			return value == null ? null : value.AttemptedValue;
		}
	}
}