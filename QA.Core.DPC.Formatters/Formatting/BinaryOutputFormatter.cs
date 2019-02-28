using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Net.Http.Headers;


namespace QA.Core.DPC.Formatters.Formatting
{
    public class BinaryOutputFormatter: OutputFormatter
    {
        private readonly IFormatter _formatter;
        
        public BinaryOutputFormatter(string mediaType)
        {
            _formatter = new BinaryFormatter();

            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(mediaType));
        }
        
        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
                        
            var response = context.HttpContext.Response;
            using (var ms = new MemoryStream())
            {
                _formatter.Serialize(ms, context.Object);
                var bytes = ms.ToArray();
                await response.Body.WriteAsync(bytes, 0 ,bytes.Length);
            }
        }
    }
}