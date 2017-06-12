using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace QA.DPC.Core.Helpers
{
    public class HeaderFormatModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }
            var contentType = bindingContext.HttpContext.Request.ContentType;

            string model;
            if (contentType == MediaTypeHeaderValues.ApplicationXml.MediaType)
            {
                model = "xml";
            }
            else if (contentType == MediaTypeHeaderValues.ApplicationJson.MediaType)
            {
                model = "json";
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Supported formats are xml or json.");
                return TaskCache.CompletedTask;
            }

            bindingContext.Result = ModelBindingResult.Success(model);
            return TaskCache.CompletedTask;
        }
    }
}
