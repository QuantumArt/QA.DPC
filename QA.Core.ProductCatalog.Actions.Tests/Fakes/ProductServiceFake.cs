using System.Collections.Generic;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using System;

namespace QA.Core.ProductCatalog.Actions.Tests.Fakes
{
	public class ProductServiceFake : IProductService
	{
		private ProductDefinition ProductDefinition { get; set; }
		public Article Article { get; set; }

		public Content Content
		{
			get { return ProductDefinition.StorageSchema; }
			set { ProductDefinition.StorageSchema = value; }
		}

		public ProductServiceFake()
		{
			ProductDefinition = new ProductDefinition();
		}
      

		public Article GetProductById(int id, bool isLive = false, ProductDefinition productDefinition = null)
		{
			return Article;
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
			return ProductDefinition;
		}

        public ProductDefinition GetProductDefinition(int productTypeId, int contentId, bool isLive = false)
		{
			return ProductDefinition;
		}

		public Dictionary<string, object>[] GetProductsList(ServiceDefinition definition, long startRow, long pageSize, bool isLive)
		{
			throw new NotImplementedException();
		}




		public Dictionary<string, object>[] GetProductsList(ServiceDefinition definition, long startRow, long pageSize, bool isLive, string query)
		{
			throw new NotImplementedException();
		}

		public Dictionary<string, object>[] GetProductsList(ServiceDefinition definition, long startRow, long pageSize, bool isLive, System.Linq.Expressions.Expression<Predicate<Quantumart.QP8.BLL.Repository.ArticleMatching.Models.IArticle>> query)
		{
			throw new NotImplementedException();
		}


		public Article[] GetSimpleProductsByIds(int[] ids, bool isLive = false)
		{
			throw new NotImplementedException();
		}
	}
}
