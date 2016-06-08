using QA.Core.Models.Entities;
using System.Globalization;

namespace QA.ProductCatalog.Infrastructure
{
    public interface IProductLocalizationService
    {
        LocalizedArticle Localize(Article product, CultureInfo culture);
        LocalizedArticle[] SplitLocalizations(Article product);
        LocalizedArticle[] SplitLocalizations(Article product, CultureInfo[] cultures);
        Article MergeLocalizations(LocalizedArticle[] localizations);
        Article MergeLocalizations(Article product, LocalizedArticle[] localizations);
    }
}
