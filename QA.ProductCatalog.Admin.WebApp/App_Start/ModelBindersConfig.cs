using System.Web.Mvc;
using QA.Core.Models.Configuration;
using QA.ProductCatalog.Admin.WebApp.Binders;
using QA.Validation.Xaml.Extensions.Rules;

namespace QA.ProductCatalog.Admin.WebApp.App_Start
{
    internal class ModelBindersConfig
    {
        internal static void Register()
        {
            ModelBinders.Binders.Add(typeof(RemoteValidationContext), new RemoteValidationContextModelBinder());

			ModelBinders.Binders.Add(typeof(Content), new ContentBinder());
        }
    }
}