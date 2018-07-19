using QA.Core.Service.Interaction;

namespace QA.Core.DPC.Integration
{
    public interface IDpcProductService
    {
        ServiceResult<bool> HasProductChanged(int id, string data);

        ServiceResult<ProductInfo> Parse(string data);

        ServiceResult UpdateProduct(Product product, string data, string userName, int userId);

        ServiceResult DeleteProduct(int id);

    }
}
