using System;
using System.Collections.Generic;

namespace QA.ProductCatalog.HighloadFront.Options
{
    public class ProductsOptions
    {
        public IList<string> PropertiesFilter { get; set; }
        public string Query { get; set; }
        public IList<Tuple<string, string>> Filters { get; set; }
        public IList<Tuple<string, string, string>> RangeFilters { get; set; }
        //TODO Simplify
        public IList<Tuple<string, IList<Tuple<string, string>>>> NestedFilters { get; set; }
        public int? Page { get; set; }
        public int? PerPage { get; set; }
        public string Sort { get; set; }
        public bool Order { get; set; }
    }
};