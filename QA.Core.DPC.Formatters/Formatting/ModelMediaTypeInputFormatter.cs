using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Formatters.Formatting
{
    public class ModelMediaTypeInputFormatter<T,TF> : TextInputFormatter
        where T : class
        where TF : class, IFormatter<T>    
    {
        public ModelMediaTypeInputFormatter(string mediaType)
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(mediaType));
            SupportedEncodings.Add(Encoding.UTF8);
        }

        protected override bool CanReadType(Type type)
        {
            return type == typeof(T);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(
            InputFormatterContext context,
            Encoding encoding
        )
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var request = context.HttpContext.Request;
            IServiceProvider serviceProvider = context.HttpContext.RequestServices;
            var formatter = (IFormatter<T>)serviceProvider.GetService(typeof(TF));            
            var result = await formatter.Read(request.Body);
            return await InputFormatterResult.SuccessAsync(result);
        }
    }
}