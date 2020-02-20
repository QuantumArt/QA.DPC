using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using System;
using System.Linq;
using System.Xml.Linq;
using NLog;
using QA.Core.Models.Configuration;

namespace QA.Core.ProductCatalog.Actions.Decorators
{
	public class XmlProductServiceProfiler : ProfilerBase, IXmlProductService
	{
		private readonly IXmlProductService _xmlProductService;

		public XmlProductServiceProfiler(IXmlProductService xmlProductService)
		{
			if (xmlProductService == null)
				throw new ArgumentNullException("xmlProductService");

			_xmlProductService = xmlProductService;
			Service = _xmlProductService.GetType().Name;
		}

		public string GetProductXml(Article article, IArticleFilter filter)
		{
			var token = CallMethod("GetProductXml", article);
            var result = _xmlProductService.GetProductXml(article, filter);
			EndMethod(token, result);
			return result;
		}

        public string GetSingleXmlForProducts(Article[] articles, IArticleFilter filter)
		{
			var token = CallMethod("GetSingleXmlForProducts", articles);
			var result = _xmlProductService.GetSingleXmlForProducts(articles, filter);
			EndMethod(token, "XML");
			return result;
		}

        public string[] GetXmlForProducts(Article[] articles, IArticleFilter filter)
		{
			var token = CallMethod("GetXmlForProducts", articles);
			var result = _xmlProductService.GetXmlForProducts(articles, filter);
			EndMethod(token, "XML");
			return result;
		}

		public bool EnableRegionTagReplacement { get; set; }
	    public Article DeserializeProductXml(XDocument productXml, Content definition)
	    {
	        throw new NotImplementedException();
	    }

	    private ProfilerToken CallMethod(string name, Article article)
		{
			return CallMethod(name, "articleId = {0}, ContentId = {1}", article.Id, article.ContentId);
		}

		private ProfilerToken CallMethod(string name, Article[] articles)
		{
			return CallMethod(name, "articles = {0}", string.Join(";", articles.Select(a => string.Format("articleId = {0}, ContentId = {1}", a.Id, a.ContentId))));
		}


        string IXmlProductService.GetProductXml(Article article, IArticleFilter filter)
        {
            return GetProductXml(article, filter);
        }

        string IXmlProductService.GetSingleXmlForProducts(Article[] articles, IArticleFilter filter)
        {
            return GetSingleXmlForProducts(articles, filter);
        }

        string[] IXmlProductService.GetXmlForProducts(Article[] articles, IArticleFilter filter)
        {
            return GetXmlForProducts(articles, filter);
        }
    }
}
