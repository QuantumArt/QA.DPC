using Newtonsoft.Json;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace QA.Core.DPC.Formatters.Services
{
    public class JsonProductArrayFormatter : IFormatter<IEnumerable<Article>>
    {
        private readonly IArticleFormatter _formatter;

        public JsonProductArrayFormatter(JsonProductFormatter formatter)
        {
            _formatter = formatter;
        }

        public Task<IEnumerable<Article>> Read(Stream stream)
        {
            throw new NotImplementedException();
        }

        public async Task Write(Stream stream, IEnumerable<Article> products)
        {
            await using TextWriter sw = new StreamWriter(stream, leaveOpen: true);
            using JsonWriter writer = new JsonTextWriter(sw) { CloseOutput = false };

            await writer.WriteStartArrayAsync();
            await writer.FlushAsync();

            foreach (var product in products)
            {
                await _formatter.Write(stream, product);
            }

            await writer.WriteEndArrayAsync();
            await writer.FlushAsync();
        }

        public string Serialize(IEnumerable<Article> products)
        {
            using (var sw = new StringWriter())
            using (var writer = new JsonTextWriter(sw))
            {
                writer.WriteStartArray();

                foreach (var product in products)
                {
                    var data = _formatter.Serialize(product);
                    using (var sr = new StringReader(data))
                    using (var reader = new JsonTextReader(sr))
                    {
                        writer.WriteToken(reader);
                        writer.Flush();
                    }
                }

                writer.WriteEndArray();
                writer.Flush();

                return sw.ToString();
            }

        }
    }
}
