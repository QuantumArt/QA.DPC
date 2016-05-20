using QA.Core.DPC.Loader;
using QA.Core.Models.Configuration;
using QA.ProductCatalog.Infrastructure;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace QA.Core.DPC.Formatters.Services
{
	public class JsonSchemaFormatter : IFormatter<Content>
	{
		private readonly IJsonProductService _jsonProductService;

		public JsonSchemaFormatter(IJsonProductService jsonProductService)
		{
			_jsonProductService = jsonProductService;
		}

		public Task<Content> Read(Stream stream)
		{
			throw new NotImplementedException();
		}

		public async Task Write(Stream stream, Content schema)
		{
			bool includeRegionTags = (bool)HttpContext.Current.Items["includeRegionTags"];

			string data = _jsonProductService.GetSchema(schema, false, includeRegionTags).ToString();

			using (var writer = new StreamWriter(stream))
			{
				await writer.WriteAsync(data);
				await writer.FlushAsync();
			}
		}
	}
}
