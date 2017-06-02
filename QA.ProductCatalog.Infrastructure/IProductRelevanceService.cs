using System;
using QA.Core.Models.Entities;
using System.Globalization;

namespace QA.ProductCatalog.Infrastructure
{
    public enum ProductRelevance:byte
    {
        Relevant,
        NotRelevant,
        Missing
    }

    public interface IProductRelevanceService
    {
        RelevanceInfo[] GetProductRelevance(Article product, bool isLive);
    }

    public class RelevanceInfo
    {
        public CultureInfo Culture { get; set; }
        public ProductRelevance Relevance { get; set; }
        public DateTime? LastPublished { get; set; }
        public string LastPublishedUserName { get; set; }
    }
}
