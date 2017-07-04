using QA.Core.Service.Interaction;

namespace QA.Core.DPC.Front
{
    public interface IDpcProductService
    {
        ServiceResult<bool> HasProductChanged(ProductLocator locator, int id, string data);

        ServiceResult<ProductInfo> Parse(ProductLocator locator, string data);

        ServiceResult UpdateProduct(ProductLocator locator, Product product, string data, string userName, int userId);

        ServiceResult DeleteProduct(ProductLocator locator, int id);

    }
}
