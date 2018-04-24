using System.IO;
using System.Threading.Tasks;
using System.Web;
using QA.Core.Models.Entities;
using QA.Core.DPC.Loader;
using QA.ProductCatalog.Infrastructure;
using QA.Core.DPC.Formatters.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Core.DPC.Loader.Services;
using QA.Core.Models.Configuration;

namespace QA.Core.DPC.Formatters.Services
{
	public class JsonProductFormatter : IArticleFormatter
	{
		private readonly IJsonProductService _jsonProductService;
		private readonly IContentDefinitionService _contentDefinitionService;
	    private readonly ISettingsService _settingsService;
        private readonly IProductContentResolver _productContentResolver;


        public JsonProductFormatter(IJsonProductService jsonProductService, IContentDefinitionService contentDefinitionService, ISettingsService settingsService, IProductContentResolver productContentResolver)
		{
			_jsonProductService = jsonProductService;
			_contentDefinitionService = contentDefinitionService;
		    _settingsService = settingsService;
            _productContentResolver = productContentResolver;
        }

		public async Task<Article> Read(Stream stream)
		{
			using (var reader = new StreamReader(stream))
			{
				string line = await reader.ReadToEndAsync();

				var context = HttpContext.Current;

				string slug = (string)context?.Request.RequestContext?.RouteData?.Values["slug"];
				string version = (string)context?.Request.RequestContext?.RouteData?.Values["version"];

                Content definition = null;

                if (slug == null && version == null)
                {
                    var type = GetTypeName(line);
                    var contentId = _productContentResolver.GetContentIdByType(type);
                    definition = _contentDefinitionService.GetDefinitionForContent(0, contentId);
                }
                else
                {
                    definition = _contentDefinitionService.GetServiceDefinition(slug, version).Content;
                }

                return _jsonProductService.DeserializeProduct(line, definition);
			}
		}

        private string GetTypeName(string product)
        {
            var json = JsonConvert.DeserializeObject<JObject>(product);
            return json.SelectToken("product.Type")?.Value<string>();
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
