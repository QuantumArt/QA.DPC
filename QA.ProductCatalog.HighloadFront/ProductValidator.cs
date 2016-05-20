using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.HighloadFront
{
    public class ProductValidator : IProductValidator
    {
        public ProductValidator(SonicErrorDescriber errors = null)
        {
            Describer = errors ?? new SonicErrorDescriber();
        }

        public SonicErrorDescriber Describer { get; private set; }

        public virtual async Task<SonicResult> ValidateAsync(ProductManager manager, JObject Product)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }
            if (Product == null)
            {
                throw new ArgumentNullException(nameof(Product));
            }
            var errors = new List<SonicError>();
            //await ValidateProductTitle(manager, Product, errors);

            return errors.Count > 0 ? SonicResult.Failed(errors.ToArray()) : SonicResult.Success;
        }

        /*
        private async Task ValidateProductTitle(ProductManager<TProduct> manager, TProduct product, ICollection<SonicError> errors)
        {
            var title = await manager.GetProductTitleAsync(product);
            if (string.IsNullOrWhiteSpace(title))
            {
                errors.Add(Describer.InvalidProductTitle(title));
            }
            else if (!string.IsNullOrEmpty(manager.Options.Product.AllowedProductTitleCharacters) &&
                title.Any(c => !manager.Options.Product.AllowedProductTitleCharacters.Contains(c)))
            {
                errors.Add(Describer.InvalidProductTitle(title));
            }
            else
            {
                var owner = await manager.FindByTitleAsync(title);
                if (owner != null &&
                    !string.Equals(await manager.GetProductIdAsync(owner), await manager.GetProductIdAsync(product)))
                {
                    errors.Add(Describer.DuplicateProductTitle(title));
                }
            }
        } */
    }
}