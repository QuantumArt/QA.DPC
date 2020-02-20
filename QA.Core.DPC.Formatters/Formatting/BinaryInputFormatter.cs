using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Net.Http.Headers;

namespace QA.Core.DPC.Formatters.Formatting
{
    public class BinaryInputFormatter: InputFormatter
    {
        private readonly IFormatter _formatter;
        
        public BinaryInputFormatter(string mediaType)
        {
            _formatter = new BinaryFormatter();
            
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(mediaType));
        }
        
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            
            var request = context.HttpContext.Request;
            var result = _formatter.Deserialize(request.Body);
            return await InputFormatterResult.SuccessAsync(result);
        }
    }
}