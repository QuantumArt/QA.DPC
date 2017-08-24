using System.IO;
using System.Threading.Tasks;
using System.Web;
using QA.Core.Models.Entities;
using QA.Core.DPC.Loader;
using QA.ProductCatalog.Infrastructure;
using QA.Core.DPC.Formatters.Configuration;

namespace QA.Core.DPC.Formatters.Services
{
	public class JsonProductFormatter : IArticleFormatter
	{
		private readonly IJsonProductService _jsonProductService;
		private readonly IContentDefinitionService _contentDefinitionService;
	    private readonly ISettingsService _settingsService;

		public JsonProductFormatter(IJsonProductService jsonProductService, IContentDefinitionService contentDefinitionService, ISettingsService settingsService)
		{
			_jsonProductService = jsonProductService;
			_contentDefinitionService = contentDefinitionService;
		    _settingsService = settingsService;
		}

		public async Task<Article> Read(Stream stream)
		{
			using (var reader = new StreamReader(stream))
			{
				string line = await reader.ReadToEndAsync();

				var context = HttpContext.Current;

				string slug = (string)context?.Request.RequestContext?.RouteData?.Values["slug"];
				string version = (string)context?.Request.RequestContext?.RouteData?.Values["version"];

				var definition = (slug == null && version == null) ? 
                    _contentDefinitionService.GetDefinitionForContent(0, int.Parse(_settingsService.GetSetting(SettingsTitles.PRODUCTS_CONTENT_ID))) :
                    _contentDefinitionService.GetServiceDefinition(slug, version).Content;

				return _jsonProductService.DeserializeProduct(line, definition);
			}
		}

		public async Task Write(Stream stream, Article product)
		{
			var articleFilter = (IArticleFilter)HttpContext.Current.Items["ArticleFilter"];
			bool includeRegionTags = (bool)HttpContext.Current.Items["includeRegionTags"];
			await this.WriteAsync(stream, product, articleFilter, includeRegionTags);
		}

		public string Serialize(Article product, IArticleFilter filter, bool includeRegionTags)
			{
			string json = _jsonProductService.SerializeProduct(product, filter, includeRegionTags);
			return json;
		}
	}
}
