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
            using (TextWriter sw = new StreamWriter(stream))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {                
                writer.WriteStartArray();

                foreach(var product in products)
                {
                    using (var productStream = new MemoryStream())
                    {
                        await _formatter.Write(productStream, product);

                        productStream.Position = 0;
                            
                        using (var sr = new StreamReader(productStream))
                        using (var reader = new JsonTextReader(sr))
                        {
                            await writer.WriteTokenAsync(reader);
                            await writer.FlushAsync();
                        }
                    }                    
                }
                                
                writer.WriteEndArray();
                writer.Flush();
            }
        }
    }
}
