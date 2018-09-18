using System;
using System.Collections.Generic;
using System.Linq;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Loader
{
    /// <summary>
    /// Fake service. Get loaded data from xaml
    /// </summary>
    public class FakeProductLoader : IProductService
    {
        private IContentDefinitionService _service;
        private const string _format = "QA.Core.DPC.Loader.Xaml.product_{0}.xaml";

        public FakeProductLoader(IContentDefinitionService service)
        {
            _service = service;
        }

      
	    public Article[] GetProductsByIds(int[] ids, bool isLive = false)
	    {
		    throw new NotImplementedException();
	    }

	    public ProductDefinition GetProductDefinition(int productTypeId, bool isLive = false)
        {
            return new ProductDefinition { StorageSchema = _service.GetDefinitionForContent(0, 288) };
        }

        #region IProductService Members

	    public Article GetProductById(int id, bool isLive = false, ProductDefinition productDefinition = null)
	    {
			return ContentDefinitionService.ResourceHelper
			   .GetXaml<Article>(string.Format(_format, id));
	    }

	    public Article[] GetProductsByIds(int contentId, int[] ids, bool isLive = false)
        {
            throw new NotImplementedException();
        }

        public Article[] GetProductsByIds(Content content, int[] articleIds, bool isLive = false)
        {
            return articleIds
                .Select(id => GetProductById(id, isLive, new ProductDefinition { StorageSchema = content }))
                .ToArray();
        }

        #endregion


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
			throw new NotImplementedException();
		}
	}
}
