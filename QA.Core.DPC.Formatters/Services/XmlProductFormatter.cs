using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using QA.Core.Models;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using QA.Core.Models.Configuration;
using QA.Core.DPC.Formatters.Configuration;

namespace QA.Core.DPC.Formatters.Services
{
	public class XmlProductFormatter : IArticleFormatter
	{
		private readonly IXmlProductService _xmlProductService;
		private readonly IContentDefinitionService _contentDefinitionService;

		public XmlProductFormatter(IXmlProductService xmlProductService, IContentDefinitionService contentDefinitionService)
		{
			_xmlProductService = xmlProductService;
			_contentDefinitionService = contentDefinitionService;
		}

		public Task<Article> Read(Stream stream)
		{
			var context = HttpContext.Current;
			return Task.Run<Article>(() => ReadProduct(stream, context));
		}

		public Article ReadProduct(Stream stream, HttpContext context)
		{
			string slug = (string)context.Request.RequestContext.RouteData.Values["slug"];
			string version = (string)context.Request.RequestContext.RouteData.Values["version"];
			var definition = _contentDefinitionService.GetServiceDefinition(slug, version);
			var productXml = XDocument.Load(stream);

			return ReadProduct(productXml, definition.Content);
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
