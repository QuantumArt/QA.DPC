using System;

namespace QA.Core.DPC.Front
{
    public interface IDpcService
    {
        int[] GetAllProductId(ProductLocator locator, int page, int pageSize);

		int[] GetLastProductId(ProductLocator locator, int page, int pageSize, DateTime date);

        string GetProduct(ProductLocator locator, int id);

        ProductData GetProductData(ProductLocator locator, int id);

        ProductData GetProductVersionData(ProductLocator locator, int id, DateTime date);

        int[] GetAllProductVersionId(ProductLocator locator, int page, int pageSize, DateTime date);
    }
}
