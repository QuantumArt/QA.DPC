using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Core.DPC.Formatters.Configuration;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Services;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Routing;
using QA.ProductCatalog.ContentProviders;

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
                Content definition = null;

                var subroutes = ((IHttpRouteData[])context.Request.RequestContext.RouteData.Values["MS_SubRoutes"]).FirstOrDefault();
                subroutes.Values.TryGetValue("slug", out object slug);
                subroutes.Values.TryGetValue("version", out object version);

                if (slug == null && version == null)
                {
                    var type = GetTypeName(line);
                    var contentId = _productContentResolver.GetContentIdByType(type);
                    definition = _contentDefinitionService.GetDefinitionForContent(0, contentId);
                }
                else
                {
                    definition = _contentDefinitionService.GetServiceDefinition((string)slug, (string)version).Content;
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
