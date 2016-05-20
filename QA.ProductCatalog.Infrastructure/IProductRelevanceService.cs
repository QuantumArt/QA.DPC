using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Models.Entities;

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
		ProductRelevance GetProductRelevance(Article product, bool isLive, out DateTime? lastPublished, out string lastPublishedUserName);
    }
}
