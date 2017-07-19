using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace QA.ProductCatalog.HighloadFront.Options
{
    public class ProductsOptions
    {
        public IList<string> PropertiesFilter { get; set; }
        public string Query { get; set; }
        public IList<SimpleFilter> Filters { get; set; }
        public IList<RangeFilter> RangeFilters { get; set; }
        public int? Page { get; set; }
        public int? PerPage { get; set; }
        public string Sort { get; set; }
        public bool Order { get; set; }
        public string[] DisableOr { get; set; }
        public string[] DisableNot { get; set; }

    }

    public class SimpleFilter
    {
        public string Name { get; set; }

        public StringValues Values { get; set; }

        public string Value => Values.FirstOrDefault();
    }

    public class RangeFilter
    {
        public string Name { get; set; }

        public string Floor { get; set; }

        public string Ceiling { get; set; }
    }
};