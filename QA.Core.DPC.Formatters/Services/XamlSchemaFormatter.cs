using System.IO;
using System.Threading.Tasks;
using Portable.Xaml;
using QA.Core.Models.Configuration;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Formatters.Services
{
	public class XamlSchemaFormatter : IFormatter<Content>
	{
		#region IFormatter implementation
		public Task<Content> Read(Stream stream)
		{
			return Task.Run<Content>(() => (Content)XamlServices.Load(stream));
		}

		public Task Write(Stream stream, Content product)
		{
			return Task.Run(() => XamlServices.Save(stream, product));
		}

		public string Serialize(Content product)
		{
			using (var stream = new MemoryStream())
			{
				XamlServices.Save(stream, product);
				return new StreamReader(stream).ReadToEnd();
			}
		}

		#endregion
	}
}
