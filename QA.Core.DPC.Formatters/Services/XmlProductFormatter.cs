using QA.Core.DPC.Formatters.Configuration;
using QA.Core.DPC.Loader.Services;
using QA.Core.Models;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using QA.ProductCatalog.ContentProviders;

namespace QA.Core.DPC.Formatters.Services
{
    public class XmlProductFormatter : IArticleFormatter
	{
		private readonly IXmlProductService _xmlProductService;
		private readonly IContentDefinitionService _contentDefinitionService;
	    private readonly ISettingsService _settingsService;
        private readonly IProductContentResolver _productContentResolver;
        private readonly HttpContext _httpContext;
        private readonly ActionContext _actionContext;

        public XmlProductFormatter(
	        IXmlProductService xmlProductService, 
	        IContentDefinitionService contentDefinitionService, 
	        ISettingsService settingsService, 
	        IProductContentResolver productContentResolver,
	        IHttpContextAccessor httpContextAccessor,
	        IActionContextAccessor actionContextAccessor
	    )
		{
			_xmlProductService = xmlProductService;
			_contentDefinitionService = contentDefinitionService;
		    _settingsService = settingsService;
            _productContentResolver = productContentResolver;
            _httpContext = httpContextAccessor?.HttpContext;
            _actionContext = actionContextAccessor?.ActionContext;
		}

		public Task<Article> Read(Stream stream)
		{
			return Task.Run<Article>(() => ReadProduct(stream));
		}

        public Article ReadProduct(Stream stream)
        {
            _actionContext.RouteData.Values.TryGetValue("slug", out object slug);
            _actionContext.RouteData.Values.TryGetValue("version", out object version);

            var productXml = XDocument.Load(stream);
            Content definition = null;

            if (slug == null && version == null)
            {
                var type = GetTypeName(productXml);
                var contentId = _productContentResolver.GetContentIdByType(type);
                definition = _contentDefinitionService.GetDefinitionForContent(0, contentId);
            }
            else
            {
                definition = _contentDefinitionService.GetServiceDefinition((string)slug, (string)version).Content;
            }

            return ReadProduct(productXml, definition);
        }

        private string GetTypeName(XDocument productXml)
        {
            return productXml.XPathSelectElement("ProductInfo/Products/Product/Type")?.Value;
        }

		public Article ReadProduct(XDocument productXml, Content definition)
		{
			return _xmlProductService.DeserializeProductXml(productXml, definition);
		}

		public async Task Write(Stream stream, Article product)
		{
			await this.WriteAsync(stream, product, ArticleFilter.DefaultFilter, true);
		}

		public Article Read(string data)
		{
			throw new System.NotImplementedException();
		}

		public string Serialize(Article product)
		{
			return Serialize(product, ArticleFilter.DefaultFilter, true);
		}

		public string Serialize(Article product, IArticleFilter filter, bool includeRegionTags)
		{
			return _xmlProductService.GetProductXml(product, filter);
		}
	}
}
