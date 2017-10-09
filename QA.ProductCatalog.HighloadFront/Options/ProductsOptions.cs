using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace QA.ProductCatalog.HighloadFront.Options
{
    public class ProductsOptions
    {
        public IList<string> PropertiesFilter { get; set; }

        public IList<IElasticFilter> Filters { get; set; }

        public IList<SimpleFilter> SimpleFilters => Filters?.OfType<SimpleFilter>().ToList();

        public IList<RangeFilter> RangeFilters => Filters?.OfType<RangeFilter>().ToList();

        public string Query => Filters?.OfType<QueryFilter>().FirstOrDefault()?.Query;

        public int? Page { get; set; }

        public int? PerPage { get; set; }

        public string Sort { get; set; }

        public bool Order { get; set; }

        public string[] DisableOr { get; set; }

        public string[] DisableNot { get; set; }

        public string[] DisableLike { get; set; }

    }

    public interface IElasticFilter
    {
        bool IsDisjunction { get; set; }

    }

    public class SimpleFilter : IElasticFilter
    {
        public string Name { get; set; }

        public StringValues Values { get; set; }

        public string Value => Values.FirstOrDefault();
        public bool IsDisjunction { get; set; }
    }

    public class RangeFilter : IElasticFilter
    {
        public string Name { get; set; }

        public string Floor { get; set; }

        public string Ceiling { get; set; }
        public bool IsDisjunction { get; set; }
    }

    public class QueryFilter : IElasticFilter
    {
        public string Query { get; set; }
        public bool IsDisjunction { get; set; }
    }

};