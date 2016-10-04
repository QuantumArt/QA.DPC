using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace QA.ProductCatalog.HighloadFront.App_Core
{
    public class ProductOptionsParser
    {
        private static Regex RangeFilterRegex { get; } = new Regex(@"\[([^&=,\[\]]*),([^&=,\[\]]*)\]");
        private static string EscapeCharacter { get; } = "@";

        public static ProductsOptions Parse(IEnumerable<KeyValuePair<string, string>> properties)
        {
            if (!properties.Any()) return null;

            var sort = properties.FirstOrDefault(p => p.Key == "sort");
            var order = properties.FirstOrDefault(p => p.Key == "order");
            var fields = properties.FirstOrDefault(p => p.Key == "fields");
            var page = properties.FirstOrDefault(p => p.Key == "page");
            var perPage = properties.FirstOrDefault(p => p.Key == "per_page");
            var query = properties.FirstOrDefault(p => p.Key == "q");
            var filters = properties
                .Except(new[] { sort, order, page, perPage, fields, query })
                .Select(p => p.Key.StartsWith(EscapeCharacter) ? 
                    new KeyValuePair<string, string>(p.Key.Substring(1), p.Value)
                    : p);

            var rangeFilters = filters.Where(f => RangeFilterRegex.IsMatch(f.Value));
            var simpleFilters = filters.Except(rangeFilters);

            var result = new ProductsOptions
            {
                Sort = sort.Value,
                Order = order.Value == "asc",
                PropertiesFilter = fields.Value?.Split(',').ToList(),
            };

            if (filters.Any())
            {
                result.Filters = simpleFilters
                    .Select(f => new Tuple<string, string>(f.Key, f.Value))
                    .ToList();
            }

            if (rangeFilters.Any())
            {
                result.RangeFilters = rangeFilters.Select(f =>
                {
                    var match = RangeFilterRegex.Match(f.Value);
                    return new Tuple<string, string, string>(
                        f.Key,
                        match.Groups[1].Value,
                        match.Groups[2].Value);
                }).ToList();
            }

            int intPage;
            if (int.TryParse(page.Value, out intPage))
            {
                result.Page = intPage;
            }

            int intPerPage;
            if (int.TryParse(perPage.Value, out intPerPage))
            {
                result.PerPage = intPerPage;
            }
            return result;
        }
    }
}