using System.Collections.Generic;
using System.Xml.Linq;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;

namespace QA.ProductCatalog.Infrastructure
{
    public interface IXmlProductService
    {
        string GetProductXml(Article article, IArticleFilter filter);
        string GetSingleXmlForProducts(Article[] articles, IArticleFilter filter);
        string[] GetXmlForProducts(Article[] articles, IArticleFilter filter);
		bool EnableRegionTagReplacement { get; set; }

        Article DeserializeProductXml(XDocument productXml, Content definition);
    }
}
