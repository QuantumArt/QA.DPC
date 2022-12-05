using QA.Core.Models.Entities;
using System;

namespace QA.Core.DPC.Loader
{
    public static class ProductDataSourceExtensions
    {
        public static string GetTmfArticleId(this IProductDataSource productDataSource)
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