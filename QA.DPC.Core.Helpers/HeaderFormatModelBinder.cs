using System;
using System.Threading.Tasks;
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
            if (contentType != null && (contentType.StartsWith(MediaTypeHeaderValues.ApplicationXml.MediaType.ToString()) || contentType.StartsWith(MediaTypeHeaderValues.TextXml.MediaType.ToString())))
            {
                model = "xml";
            }
            else if (contentType != null && contentType.StartsWith(MediaTypeHeaderValues.ApplicationJson.MediaType.ToString()))
            {
                model = "json";
            }
            else
            {
                model = null;
            }

            bindingContext.Result = ModelBindingResult.Success(model);
            return Task.CompletedTask;
        }
    }
}
