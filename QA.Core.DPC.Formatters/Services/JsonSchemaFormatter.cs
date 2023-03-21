using QA.Core.DPC.Loader;
using QA.Core.Models.Configuration;
using QA.ProductCatalog.Infrastructure;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace QA.Core.DPC.Formatters.Services
{
    public class JsonSchemaFormatter : IFormatter<Content>
    {
        private readonly IJsonProductService _jsonProductService;
        private readonly HttpContext _httpContext;

        public JsonSchemaFormatter(IJsonProductService jsonProductService, IHttpContextAccessor httpContextAccessor)
        {
            _jsonProductService = jsonProductService;
            _httpContext = httpContextAccessor.HttpContext;
        }

        public Task<Content> Read(Stream stream)
        {
            throw new NotImplementedException();
        }

        public async Task Write(Stream stream, Content schema)
        {
            var data = Serialize(schema);
            await using var writer = new StreamWriter(stream, leaveOpen: true);
            await writer.WriteAsync(data);
            await writer.FlushAsync();
        }

        public string Serialize(Content schema)
        {
            bool includeRegionTags = (bool) _httpContext.Items["includeRegionTags"];
            return _jsonProductService.GetSchema(schema, false, includeRegionTags).ToString();
        }
    }
}
