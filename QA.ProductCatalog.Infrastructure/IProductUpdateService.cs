using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using Quantumart.QP8.BLL.Services.API.Models;
using System;

namespace QA.ProductCatalog.Infrastructure
{
	public interface IProductUpdateService
	{
        /// <exception cref="ProductUpdateConcurrencyException" />
		InsertData[] Update(Article product, ProductDefinition definition, bool isLive = false);
	}

    public sealed class ProductUpdateConcurrencyException : Exception
    {
    }
}