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
	    private readonly ISettingsService _settingsService;

        public XmlProductFormatter(IXmlProductService xmlProductService, IContentDefinitionService contentDefinitionService, ISettingsService settingsService)
		{
			_xmlProductService = xmlProductService;
			_contentDefinitionService = contentDefinitionService;
		    _settingsService = settingsService;
		}

		public Task<Article> Read(Stream stream)
		{
			var context = HttpContext.Current;
			return Task.Run<Article>(() => ReadProduct(stream, context));
		}

		public Article ReadProduct(Stream stream, HttpContext context)
		{
		    string slug = (string)context?.Request.RequestContext?.RouteData?.Values["slug"];
		    string version = (string)context?.Request.RequestContext?.RouteData?.Values["version"];

            var productXml = XDocument.Load(stream);
            var definition = (slug == null && version == null) ?
		        _contentDefinitionService.GetDefinitionForContent(0, int.Parse(_settingsService.GetSetting(SettingsTitles.PRODUCTS_CONTENT_ID))) :
		        _contentDefinitionService.GetServiceDefinition(slug, version).Content;
			return ReadProduct(productXml, definition);
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
