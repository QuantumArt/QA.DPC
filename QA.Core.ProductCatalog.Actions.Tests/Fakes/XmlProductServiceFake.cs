using System;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using System.Linq;
using System.Xml.Linq;

namespace QA.Core.ProductCatalog.Actions.Tests.Fakes
{
	public class XmlProductServiceFake : IXmlProductService
	{
        public string GetProductXml(Article article, IArticleFilter filter)
		{
			return string.Empty;
		}

		public string GetSingleXmlForProducts(Article[] articles, IArticleFilter filter)
		{
			return string.Empty;
		}

        public string[] GetXmlForProducts(Article[] articles, IArticleFilter filter)
		{
			return articles.Select(a => string.Empty).ToArray();
		}

        #region IXmlProductService Members

        public string[] GetXmlIndexList(int[] ids)
        {
            return ids.Select(a => string.Empty).ToArray();
        }

        #endregion
		public bool EnableRegionTagReplacement
		{
			get
			{
				// TODO: Implement this property getter
				throw new NotImplementedException();
			}
			set
			{
				// TODO: Implement this property setter
				throw new NotImplementedException();
			}
		}

		public Article DeserializeProductXml(XDocument productXml, Content definition)
		{
			throw new NotImplementedException();
		}
	}
}
