using System;
using System.Collections.Generic;
using System.Linq;

namespace QA.Core.DPC.Front.DAL
{
    public partial class DpcModelDataContext
    {
        public Product GetProduct(ProductLocator locator, int id)
        {
            return GetProducts(locator).FirstOrDefault(m => id == m.DpcId);
        }

        public IQueryable<Product> GetProducts(ProductLocator locator)
        {
            if (string.IsNullOrEmpty(locator.Slug))
            {
                return Products.Where(m =>
                    m.IsLive == locator.IsLive &&
                    m.Language == locator.Language &&
                    m.Format == locator.Format &&
                    m.Version == locator.Version &&
                    m.Slug.Equals(null));
            }
            return Products.Where(m =>
                m.IsLive == locator.IsLive &&
                m.Language == locator.Language &&
                m.Format == locator.Format &&
                m.Version == locator.Version &&
                m.Slug == locator.Slug);
        }

        public void FillProduct(ProductLocator locator, Product product)
        {
            product.Format = locator.Format;
            product.IsLive = locator.IsLive;
            product.Language = locator.Language;
            product.Slug = !string.IsNullOrEmpty(locator.Slug) ? locator.Slug : null;
            product.Version = locator.Version;
        }
    }
}
