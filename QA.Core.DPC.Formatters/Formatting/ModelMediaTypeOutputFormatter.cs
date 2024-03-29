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
    public class ModelMediaTypeOutputFormatter<T, TF> : TextOutputFormatter
        where T : class 
        where TF : class, IFormatter<T>
    {

        public ModelMediaTypeOutputFormatter(string mediaType)
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(mediaType));
            SupportedEncodings.Add(Encoding.UTF8);
        }

        protected override bool CanWriteType(Type type)
        {
            return type.IsAssignableTo(typeof(T));
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            
            var response = context.HttpContext.Response;
            IServiceProvider serviceProvider = context.HttpContext.RequestServices;
            var formatter = (IFormatter<T>)serviceProvider.GetService(typeof(TF));
            await formatter.Write(response.Body, (T)context.Object);
        }
    }
}