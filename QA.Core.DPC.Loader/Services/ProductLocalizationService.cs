using QA.ProductCatalog.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Models.Entities;
using System.Globalization;

namespace QA.Core.DPC.Loader.Services
{

    class ProductLocalizationService : IProductLocalizationService
    {
        #region IProductLocalizationService implementation
        public LocalizedArticle Localize(Article product, CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            return new LocalizedArticle(product, culture);
        }

        public Article MergeLocalizations(LocalizedArticle[] localizations)
        {
            throw new NotImplementedException();
        }

        public Article MergeLocalizations(Article product, LocalizedArticle[] localizations)
        {
            throw new NotImplementedException();
        }

        public LocalizedArticle[] SplitLocalizations(Article product)
        {
            throw new NotImplementedException();
        }

        public LocalizedArticle[] SplitLocalizations(Article product, CultureInfo[] cultures)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
