using System;
using System.Collections.Generic;
using System.Linq;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.API.Test.Fakes
{
	public class ProductServiceFake : IProductService
	{
		public Article Product { get; set; }

		#region IProductService implementation

		public Article GetProductById(int id, bool isLive = false, ProductDefinition productDefinition = null)
		{
			return Product;
		}

		public Article[] GetProductsByIds(int contentId, int[] ids, bool isLive = false)
		{
			throw new NotImplementedException();
		}

		public Article[] GetProductsByIds(int[] ids, bool isLive = false)
		{
			throw new NotImplementedException();
		}

        public Article[] GetProductsByIds(Content content, int[] articleIds, bool isLive = false)
        {
            throw new NotImplementedException();
        }

        public ProductDefinition GetProductDefinition(int productTypeId, bool isLive = false)
		{
			throw new NotImplementedException();
		}

		public ProductDefinition GetProductDefinition(int productTypeId, int contentId, bool isLive = false)
		{
			throw new NotImplementedException();
		}

		public Dictionary<string, object>[] GetProductsList(ServiceDefinition definition, long startRow, long pageSize, bool isLive)
		{
			throw new NotImplementedException();
		}

		public Article[] GetSimpleProductsByIds(int[] ids, bool isLive = false)
		{
			return ids.Select(n => new Article() {Id = n}).ToArray();
		}

		#endregion
	}
}
