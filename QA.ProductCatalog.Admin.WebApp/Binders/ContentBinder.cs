using System;
using System.Web.Mvc;
using QA.Core.Models.Configuration;

namespace QA.ProductCatalog.Admin.WebApp.Binders
{
    public class ContentBinder : IModelBinder
	{
		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
            var content = new Content();

            if (TryGetString(bindingContext, nameof(Content.ContentName), out string contentName))
            {
                content.ContentName = contentName;
            }
            if (TryGetValue(bindingContext, "CacheEnabled", out bool cacheEnabled) && cacheEnabled)
            {
                if (TryGetValue(bindingContext, "CachePeriod", out TimeSpan cachePeriod))
                {
                    XmlMappingBehavior.SetCachePeriod(content, cachePeriod);
                }
            }
            if (TryGetValue(bindingContext, nameof(Content.IsReadOnly), out bool isReadOnly))
            {
                content.IsReadOnly = isReadOnly;
            }
            if (TryGetValue(bindingContext, nameof(Content.LoadAllPlainFields), out bool loadAllPlainFields))
            {
                content.LoadAllPlainFields = loadAllPlainFields;
            }
            if (TryGetValue(bindingContext, nameof(Content.PublishingMode), out PublishingMode publishingMode))
            {
                content.PublishingMode = publishingMode;
            }

			return content;
		}

        private bool TryGetValue<T>(ModelBindingContext bindingContext, string name, out T value)
        {
            var result = bindingContext.ValueProvider.GetValue($"{bindingContext.ModelName }.{name}");
            if (result == null)
            {
                value = default(T);
                return false;
            }
            value = (T)result.ConvertTo(typeof(T));
            return true;
        }

        private bool TryGetString(ModelBindingContext bindingContext, string name, out string value)
        {
            var result = bindingContext.ValueProvider.GetValue($"{bindingContext.ModelName }.{name}");
            if (result == null || String.IsNullOrWhiteSpace(result.AttemptedValue))
            {
                value = null;
                return false;
            }
            value = result.AttemptedValue;
            return true;
        }
    }
}