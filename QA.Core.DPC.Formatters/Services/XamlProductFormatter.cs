using System.IO;
using System.Threading.Tasks;
#if !NETSTANDARD
using System.Xaml;
#else
using Portable.Xaml;
#endif
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Formatters.Services
{
	public class XamlProductFormatter : IArticleFormatter
	{
		#region IArticleFormatter implementation
		public Task<Article> Read(Stream stream)
		{
			return Task.Run<Article>(() => (Article)XamlServices.Load(stream));
		}

		public Task Write(Stream stream, Article product)
		{
			return Task.Run(() => XamlServices.Save(stream, product));
		}

		public string Serialize(Article product)
		{
			using (var writer = new StringWriter())
			{
				XamlServices.Save(writer, product);
				return writer.ToString();
			}
		}

		public string Serialize(Article product, IArticleFilter filter, bool includeRegionTags)
		{
			return Serialize(product);
		}
		#endregion	
	}
}
