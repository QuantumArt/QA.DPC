using QA.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using System.IO;
using System.Threading.Tasks;

namespace QA.Core.DPC.Formatters.Services
{
    public class XamlProductFormatter : IArticleFormatter
    {
        #region IArticleFormatter implementation
        public Task<Article> Read(Stream stream)
        {
            return Task.FromResult((Article)XamlConfigurationParser.LoadFrom(stream));
        }

        public async Task Write(Stream stream, Article product)
        {
            using var memoryStream = new MemoryStream();
            XamlConfigurationParser.SaveTo(memoryStream, product);
            await stream.CopyToAsync(memoryStream);
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
