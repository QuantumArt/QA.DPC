using QA.Core.Models.Configuration;

namespace QA.Core.DPC.API.Search
{
    public class ProductQuery
    {
        public string Query { get; set; }
        public Content Definition { get; set; }
        public int[] ExstensionContentIds { get; set; }
    }
}
