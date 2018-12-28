using QA.ProductCatalog.Infrastructure;
using System;

namespace QA.ProductCatalog.WebApi.Models
{
    public class Product
    {
        public Product(RelevanceInfo info)
        {
            Id = info.ProductId;
            Relevance = info.Relevance.ToString();
            LastPublished = info.LastPublished;
            LastPublishedUserName = info.LastPublishedUserName;
        }

        public int Id { get; set; }
        public string Relevance { get; set; }
        public DateTime? LastPublished { get; set; }
        public string LastPublishedUserName { get; set; }
    }
}