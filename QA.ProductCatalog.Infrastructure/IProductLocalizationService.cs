﻿using QA.Core.Models.Entities;
using System.Collections.Generic;
using System.Globalization;

namespace QA.ProductCatalog.Infrastructure
{
    public interface IProductLocalizationService
    {
        Article Localize(Article product, CultureInfo culture);
        Dictionary<CultureInfo, Article> SplitLocalizations(Article product, bool localize);
        Dictionary<CultureInfo, Article> SplitLocalizations(Article product, CultureInfo[] cultures, bool localize);
        CultureInfo[] GetCultures();
    }
}
