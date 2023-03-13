using QA.Core.Models.Entities;

namespace QA.ProductCatalog.TmForum.Extensions
{
    internal static class ProductDataSourceExtension
    {
        internal static string GetTmfArticleId(this IProductDataSource productDataSource)
        {
            if (productDataSource is null)
            {
                throw new ArgumentNullException(nameof(productDataSource));
            }

            try
            {
                return productDataSource.GetString(nameof(Article.Id));
            }
            catch (InvalidCastException)
            {
                return default;
            }
        }
    }
}
