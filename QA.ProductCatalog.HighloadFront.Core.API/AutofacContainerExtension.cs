using Autofac;
using QA.Core.DPC.QP.Services;

namespace QA.ProductCatalog.HighloadFront.Core.API
{
    public static class AutofacContainerExtensions
    {
        public static string GetCustomerCode(this IComponentContext ctx)
        {
            return ctx.Resolve<IIdentityProvider>().Identity.CustomerCode;
        }
    }

}
