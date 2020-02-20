using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Core.DPC.Formatters.Configuration;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Services;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using QA.ProductCatalog.ContentProviders;

namespace QA.Core.DPC.Formatters.Services
{
    public class JsonProductFormatter : IArticleFormatter
	{
		private readonly IJsonProductService _jsonProductService;
		private readonly IContentDefinitionService _contentDefinitionService;
	    private readonly ISettingsService _settingsService;
        private readonly IProductContentResolver _productContentResolver;
        private readonly HttpContext _httpContext;
        private readonly ActionContext _actionContext;


        public JsonProductFormatter(
	        IJsonProductService jsonProductService, 
	        IContentDefinitionService contentDefinitionService, 
	        ISettingsService settingsService, 
	        IProductContentResolver productContentResolver,
	        IHttpContextAccessor httpContextAccessor,
	        IActionContextAccessor actionContextAccessor
	    )
		{
			_jsonProductService = jsonProductService;
			_contentDefinitionService = contentDefinitionService;
		    _settingsService = settingsService;
            _productContentResolver = productContentResolver;
            _httpContext = httpContextAccessor?.HttpContext;
            _actionContext = actionContextAccessor?.ActionContext;
		}

        public async Task<Article> Read(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                string line = await reader.ReadToEndAsync();
                Content definition = null;

                _actionContext.RouteData.Values.TryGetValue("slug", out object slug);
                _actionContext.RouteData.Values.TryGetValue("version", out object version);

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
			var articleFilter = (IArticleFilter)_httpContext.Items["ArticleFilter"];
			bool includeRegionTags = (bool)_httpContext.Items["includeRegionTags"];
			await this.WriteAsync(stream, product, articleFilter, includeRegionTags);
		}
		
		public string Serialize(Article product)
		{
			var articleFilter = (IArticleFilter)_httpContext.Items["ArticleFilter"];
			bool includeRegionTags = (bool)_httpContext.Items["includeRegionTags"];			
			return Serialize(product, articleFilter, includeRegionTags);
		}

		public string Serialize(Article product, IArticleFilter filter, bool includeRegionTags)
			{
			string json = _jsonProductService.SerializeProduct(product, filter, includeRegionTags);
			return json;
		}
	}
}
