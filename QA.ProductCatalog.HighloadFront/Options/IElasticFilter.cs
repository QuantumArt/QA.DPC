using System.Linq;

namespace QA.ProductCatalog.HighloadFront.Options
{
    public interface IElasticFilter
    {
        bool IsDisjunction { get; set; }
    }

    public class SimpleFilter : IElasticFilter
    {
        public string Name { get; set; }
        public string[] Values { get; set; }
        public string Value => Values.FirstOrDefault();
        public bool IsDisjunction { get; set; }
        public bool FromJson { get; set; }
        
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

    public class GroupFilter : IElasticFilter
    {
        public IElasticFilter[] Filters { get; set; }
        public bool IsDisjunction { get; set; }
    }
}
