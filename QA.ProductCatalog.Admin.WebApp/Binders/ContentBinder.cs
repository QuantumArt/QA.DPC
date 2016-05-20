using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Web.Mvc;
using QA.Core.Models.Configuration;

namespace QA.ProductCatalog.Admin.WebApp.Binders
{
	public class ContentBinder : IModelBinder
	{
		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			var content = new Content
			{
				ContentName = bindingContext.ValueProvider.GetValue(bindingContext.ModelName + "." + "ContentName").AttemptedValue
			};

			var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName + ".CacheEnabled");

			bool cacheEnabled = valueProviderResult != null && (bool) valueProviderResult.ConvertTo(typeof (bool));

			TimeSpan? cachePeriod = null;

			if (cacheEnabled)
				cachePeriod = (TimeSpan) bindingContext.ValueProvider.GetValue(bindingContext.ModelName + ".CachePeriod").ConvertTo(typeof (TimeSpan));

			XmlMappingBehavior.SetCachePeriod(content, cachePeriod);

			var loadAllPlainFieldsVal = bindingContext.ValueProvider.GetValue(bindingContext.ModelName + "." + "LoadAllPlainFields");

			if (loadAllPlainFieldsVal != null)
				content.LoadAllPlainFields = (bool) loadAllPlainFieldsVal.ConvertTo(typeof (bool));

			var publishingModeVal = bindingContext.ValueProvider.GetValue(bindingContext.ModelName + "." + "PublishingMode");

			if (publishingModeVal != null)
				content.PublishingMode = (PublishingMode) publishingModeVal.ConvertTo(typeof (PublishingMode));

			return content;
		}
	}
}