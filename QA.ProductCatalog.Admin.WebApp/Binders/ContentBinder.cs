﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using QA.Core.Models.Configuration;

namespace QA.ProductCatalog.Admin.WebApp.Binders
{
    public class ContentBinder : IModelBinder
	{
        public Task BindModelAsync(ModelBindingContext bindingContext)
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
                    content.CachePeriod = cachePeriod;
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

            bindingContext.Result = ModelBindingResult.Success(content);

			return Task.CompletedTask;
		}

        private bool TryGetValue<T>(ModelBindingContext bindingContext, string name, out T value)
        {
            var result = bindingContext.ValueProvider.GetValue($"{bindingContext.ModelName }.{name}");
            if (result == ValueProviderResult.None)
            {
                value = default(T);
                return false;
            }
            value = (T)( new TypeConverter().ConvertTo(result.FirstValue, typeof(T)));
            return true;
        }

        private bool TryGetString(ModelBindingContext bindingContext, string name, out string value)
        {
            var result = bindingContext.ValueProvider.GetValue($"{bindingContext.ModelName }.{name}");
            if (result == ValueProviderResult.None)
            {
                value = null;
                return false;
            }
            value = result.FirstValue;
            return true;
        }
    }

    public class ContentBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(Content))
            {
                return new ContentBinder();
            }

            return null;
        }
    }
}