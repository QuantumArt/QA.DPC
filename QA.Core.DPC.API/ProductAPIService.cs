using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.Core.ProductCatalog.Actions;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;
using QA.Core.Models.Extensions;
using System.Linq;

namespace QA.Core.DPC.API
{
    public class ProductAPIService : IProductAPIService
	{
		private readonly IContentDefinitionService _contentDefinitionService;
		private readonly IProductService _productService;
		private readonly IProductSearchService _productSearchService;
		private readonly IProductUpdateService _productUpdateService;
		private readonly IUserProvider _userProvider;
		private readonly Func<string, IAction> _getAction;
        private readonly IProductRelevanceService _relevanceService;

        public ProductAPIService
		(
			IContentDefinitionService contentDefinitionService,
			IProductService productService,
			IProductSearchService productSearchService,
			IProductUpdateService productUpdateService,
			IUserProvider userProvider,
			Func<string, IAction> getAction,
            IProductRelevanceService relevanceService
		)
		{
			_contentDefinitionService = contentDefinitionService;
			_productService = productService;
			_productSearchService = productSearchService;
			_productUpdateService = productUpdateService;
			_userProvider = userProvider;
			_getAction = getAction;
            _relevanceService = relevanceService;
        }

		#region IProductAPIService implementation
		public Dictionary<string, object>[] GetProductsList(string slug, string version, bool isLive = false, long startRow = 0, long pageSize = int.MaxValue)
		{
			var definition = _contentDefinitionService.GetServiceDefinition(slug, version);
			return _productService.GetProductsList(definition, startRow, pageSize, isLive);
		}

		public int[] SearchProducts(string slug, string version, string query, bool isLive = false)
		{
			return _productSearchService.SearchProducts(slug, version, query);
		}

        public int[] ExtendedSearchProducts(string slug, string version, JToken query, bool isLive = false)
        {
            return _productSearchService.ExtendedSearchProducts(slug, version, query);
        }

        public Article GetProduct(string slug, string version, int id, bool isLive = false, bool includeRelevanceInfo = false)
		{
			var definition = _contentDefinitionService.GetServiceDefinition(slug, version);
			var product = _productService.GetProductById(id, isLive, new ProductDefinition { ProdictTypeId = 0, StorageSchema = definition.Content });

            if (includeRelevanceInfo)
            {
                var relevance = _relevanceService.GetProductRelevance(product, isLive, false).FirstOrDefault();

                if (relevance != null)
                {
                    product.AddPlainField("Relevance", relevance.Relevance.ToString());
                    product.AddPlainField("LastPublished", relevance.LastPublished);
                    product.AddPlainField("LastPublishedUserName", relevance.LastPublishedUserName);
                }
            }

            return product;
		}

		public void UpdateProduct(string slug, string version, Article product, bool isLive = false)
		{
			var definition = _contentDefinitionService.GetServiceDefinition(slug, version);
			_productUpdateService.Update(product, new ProductDefinition { ProdictTypeId = 0, StorageSchema = definition.Content }, isLive);
		}

		public void CustomAction(string actionName, int id, int contentId = default(int))
		{
			CustomAction(actionName, new[] { id }, new Dictionary<string, string>(), contentId);
		}

		public void CustomAction(string actionName, int id, Dictionary<string, string> parameters, int contentId = default(int))
		{
			CustomAction(actionName, new[] { id }, parameters, contentId);
		}

		public void CustomAction(string actionName, int[] ids, int contentId = default(int))
		{
			CustomAction(actionName, ids, new Dictionary<string, string>(), contentId);
		}

		public void CustomAction(string actionName, int[] ids, Dictionary<string, string> parameters, int contentId = default(int))
		{
			int userId = _userProvider.GetUserId();
		    string userName = _userProvider.GetUserName();
			var action = _getAction(actionName);
			var context = new ActionContext
			{
			    ContentItemIds = ids,
                UserId = userId,
                UserName = userName,
                Parameters = parameters ?? new Dictionary<string, string>(),
                ContentId = contentId
			};
			action.Process(context);
		}

		public ServiceDefinition GetProductDefinition(string slug, string version, bool forList = false)
		{
			return _contentDefinitionService.GetServiceDefinition(slug, version, true);
		}

        public RelevanceInfo GetRelevance(int id, bool isLive = false)
        {
            var simpleProduct = _productService.GetSimpleProductsByIds(new[] { id }, isLive).FirstOrDefault();

            if (simpleProduct == null || simpleProduct.ContentId == 0)
            {
                return null;
            }

            var definition = _productService.GetProductDefinition(0, simpleProduct.ContentId, isLive);
            var product = _productService.GetProductById(id, isLive, definition);
             
            return _relevanceService.GetProductRelevance(product, isLive, false).FirstOrDefault();
        }
        #endregion
    }
}