using QA.Core.DPC.Formatters.Configuration;
using QA.Core.DPC.Loader.Services;
using QA.Core.Models;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Routing;
using System.Xml.Linq;
using System.Xml.XPath;
using QA.ProductCatalog.ContentProviders;

namespace QA.Core.DPC.Formatters.Services
{
    public class XmlProductFormatter : IArticleFormatter
	{
		private readonly IXmlProductService _xmlProductService;
		private readonly IContentDefinitionService _contentDefinitionService;
	    private readonly ISettingsService _settingsService;
        private readonly IProductContentResolver _productContentResolver;

        public XmlProductFormatter(IXmlProductService xmlProductService, IContentDefinitionService contentDefinitionService, ISettingsService settingsService, IProductContentResolver productContentResolver)
		{
			_xmlProductService = xmlProductService;
			_contentDefinitionService = contentDefinitionService;
		    _settingsService = settingsService;
            _productContentResolver = productContentResolver;
        }

		public Task<Article> Read(Stream stream)
		{
			var context = HttpContext.Current;
			return Task.Run<Article>(() => ReadProduct(stream, context));
		}

        public Article ReadProduct(Stream stream, HttpContext context)
        {
            var subroutes = ((IHttpRouteData[])context.Request.RequestContext.RouteData.Values["MS_SubRoutes"]).FirstOrDefault();
            subroutes.Values.TryGetValue("slug", out object slug);
            subroutes.Values.TryGetValue("version", out object version);

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

		public string Serialize(Article product, IArticleFilter filter, bool includeRegionTags)
		{
			return _xmlProductService.GetProductXml(product, filter);
		}
	}
}
