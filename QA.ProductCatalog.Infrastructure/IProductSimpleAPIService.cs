﻿using QA.Core.Models.Entities;

namespace QA.ProductCatalog.Infrastructure
{
    public interface IProductSimpleAPIService
    {
        Article GetProduct(int productId, int definitionId, bool isLive = false);
    }
}
