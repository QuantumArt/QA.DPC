using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Portable.Xaml;
using QA.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Formatters.Services
{
	public class XamlProductFormatter : IArticleFormatter
	{
		#region IArticleFormatter implementation
		public Task<Article> Read(Stream stream)
		{
			return Task.Run<Article>(() => (Article)XamlConfigurationParser.LoadFrom(stream));
		}

		public Task Write(Stream stream, Article product)
		{
			return Task.Run(() => XamlConfigurationParser.SaveTo(stream, product));
		}

		public string Serialize(Article product)
		{
			return XamlConfigurationParser.Save(product);
		}

		public string Serialize(Article product, IArticleFilter filter, bool includeRegionTags)
		{
			return Serialize(product);
		}
		#endregion	
	}
}
