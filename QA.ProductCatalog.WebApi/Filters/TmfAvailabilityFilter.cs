using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace QA.ProductCatalog.WebApi.Filters
{
    public class TmfAvailabilityFilter : IDocumentFilter, IAuthorizationFilter
    {
        private readonly bool _isTmfEnabled;

        public TmfAvailabilityFilter(TMForumSettings tmfSettings)
        {
            _isTmfEnabled = tmfSettings?.IsEnabled ?? false;
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var api in context.ApiDescriptions)
            {
                var attribute = api.CustomAttributes().OfType<TmfProductFormatAttribute>().FirstOrDefault();

                if (attribute != null && !_isTmfEnabled)
                {
                    var route = "/" + api.RelativePath;
                    _ = swaggerDoc.Paths.Remove(route);
                }
            }
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var isTmfController = HasCustomAttribute<TmfProductFormatAttribute>(
                context.ActionDescriptor as ControllerActionDescriptor);

            if (isTmfController && !_isTmfEnabled)
            {
                context.Result = new NotFoundResult();
            }
        }

        private static bool HasCustomAttribute<TAttribute>(ControllerActionDescriptor controllerActionDescriptor)
        {
            if (controllerActionDescriptor is null)
            {
                return false;
            }

            // Check if the attribute exists on the action method
            if (controllerActionDescriptor.MethodInfo?.GetCustomAttributes(true)?.Any(a => a.GetType().Equals(typeof(TAttribute))) ?? false)
            {
                return true;
            }

            // Check if the attribute exists on the controller
            if (controllerActionDescriptor.ControllerTypeInfo?.GetCustomAttributes(typeof(TAttribute), true)?.Any() ?? false)
            {
                return true;
            }

            return false;
        }
    }
}
