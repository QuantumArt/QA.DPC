using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QA.Core.Models;
using QA.ProductCatalog.TmForum.Models;

namespace QA.ProductCatalog.TmForum.Filters
{
    public class TmfProductFormatAttribute : TypeFilterAttribute
    {
        public TmfProductFormatAttribute() : base(typeof(TmfProductFormatFilter))
        {
        }

        private class TmfProductFormatFilter : IAuthorizationFilter
        {
            public void OnAuthorization(AuthorizationFilterContext context)
            {
                context.HttpContext.Items[InternalTmfSettings.TmfItemIdentifier] = true;
                context.HttpContext.Items[InternalTmfSettings.ArticleFilterContextItemName] = ArticleFilter.DefaultFilter;
                context.HttpContext.Items[InternalTmfSettings.RegionTagsContextItemName] = false;
            }
        }
    }
}
