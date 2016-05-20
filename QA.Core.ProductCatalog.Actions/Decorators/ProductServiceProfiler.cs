using System.Collections.Generic;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using System;

namespace QA.Core.ProductCatalog.Actions.Decorators
{
	public class ProductServiceProfiler : ProfilerBase, IProductService
	{
		private readonly IProductService _productService;

		public ProductServiceProfiler(IProductService productService, ILogger logger)
			: base(logger)
		{
			if (productService == null)
				throw new ArgumentNullException("productService");

			_productService = productService;
			Service = _productService.GetType().Name;
		}

		public Article GetProductById(int id, bool isLive = false, ProductDefinition productDefinition = null)
		{
			var token = CallMethod("GetProductById", "id = {0}, isLive = {1}", id, isLive);
			var result = _productService.GetProductById(id, isLive, productDefinition);
			EndMethod(token, result);
			return result;
		}


		public Article[] GetProductsByIds(int contentId, int[] ids, bool isLive = false)
		{
			var token = CallMethod();
			var result = _productService.GetProductsByIds(contentId, ids, isLive);
			EndMethod(token, "Article[]");
			return result;
		}

		public Article[] GetProductsByIds(int[] ids, bool isLive = false)
		{
			var token = CallMethod();
			var result = _productService.GetProductsByIds(ids, isLive);
			EndMethod(token, "Article[]");
			return result;
		}

		public ProductDefinition GetProductDefinition(int productTypeId, bool isLive = false)
		{
			var token = CallMethod();
			var result = _productService.GetProductDefinition(productTypeId, isLive);
			EndMethod(token, result);
			return result;
		}

		public ProductDefinition GetProductDefinition(int productTypeId, int contentId, bool isLive = false)
		{
			var token = CallMethod();
			var result = _productService.GetProductDefinition(productTypeId, contentId, isLive);
			EndMethod(token, result);
			return result;			
		}

		public Dictionary<string, object>[] GetProductsList(ServiceDefinition definition, long startRow, long pageSize, bool isLive)
		{
			throw new NotImplementedException();
		}


		private void EndMethod(ProfilerToken token, Article result)
		{
			EndMethod(token, "Id = {0}, ContentId = {1}", result.Id, result.ContentId);
		}

		private void EndMethod(ProfilerToken token, ProductDefinition definition)
		{
			EndMethod(token, "ProductDefinition = {0}", definition.ProdictTypeId);
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
