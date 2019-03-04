using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using QA.Validation.Xaml.Extensions.Rules;

namespace QA.ProductCatalog.Admin.WebApp.Binders
{
    public class RemoteValidationContextModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            string body;

            if (bindingContext.HttpContext.Request.Body.Position > 0)
                bindingContext.HttpContext.Request.Body.Position = 0;

            using (var reader = new StreamReader(bindingContext.HttpContext.Request.Body))
            {
                body = reader.ReadToEnd();
            }

            var result = JsonConvert.DeserializeObject<RemoteValidationContext>(body);
            bindingContext.Result = ModelBindingResult.Success(result);
            return Task.CompletedTask;
        }
    }

    public class RemoteValidationContextModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(RemoteValidationContext))
            {
                return new RemoteValidationContextModelBinder();
            }

            return null;
        }
    }
}
