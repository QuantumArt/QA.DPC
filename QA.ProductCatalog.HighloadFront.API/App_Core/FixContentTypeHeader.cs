using System.Threading.Tasks;
using Microsoft.Owin;

namespace QA.ProductCatalog.HighloadFront.App_Core
{
    public class FixContentTypeHeader : OwinMiddleware
    {
        public FixContentTypeHeader(OwinMiddleware next) : base(next) { }

        public override Task Invoke(IOwinContext context)
        {
            if (!string.IsNullOrWhiteSpace(context.Request.ContentType))
            {
                context.Request.ContentType = "application/json";
            }

            return Next.Invoke(context);
        }
    }
}