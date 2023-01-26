using System.IO;
using System.Threading.Tasks;
using QA.Configuration;
using QA.Core.Models.Configuration;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Formatters.Services
{
    public class XamlSchemaFormatter : IFormatter<Content>
    {
        #region IFormatter implementation
        public Task<Content> Read(Stream stream)
        {
            return Task.FromResult((Content)XamlConfigurationParser.LoadFrom(stream));
        }

        public async Task Write(Stream stream, Content product)
        {
            using var memoryStream = new MemoryStream();
            XamlConfigurationParser.SaveTo(memoryStream, product);
            await stream.CopyToAsync(memoryStream);
        }

        public string Serialize(Content product)
        {
            using (var stream = new MemoryStream())
            {
                XamlConfigurationParser.SaveTo(stream, product);
                return new StreamReader(stream).ReadToEnd();
            }
        }

        #endregion
    }
}
