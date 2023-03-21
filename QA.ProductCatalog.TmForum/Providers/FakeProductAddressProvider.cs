using Microsoft.AspNetCore.Http;
using QA.ProductCatalog.TmForum.Interfaces;
using QA.ProductCatalog.TmForum.Models;

namespace QA.ProductCatalog.TmForum.Providers
{
    public class FakeProductAddressProvider : IProductAddressProvider
    {
        public Uri GetProductAddress(string type, string resourceId)
        {
            UriBuilder uriBuilder = new(InternalTmfSettings.FakeDpcApiAddress)
            {
                Path = new PathString($"/{type}/{resourceId}")
            };
            return uriBuilder.Uri;
        }
    }
}
