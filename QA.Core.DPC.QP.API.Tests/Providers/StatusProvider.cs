using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.QP.API.Tests.Providers
{
    public class StatusProvider : IStatusProvider
    {
        public string GetStatusName(int contentId, int statusId)
        {
            return "Published";
        }
    }
}
