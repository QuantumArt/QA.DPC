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
    public class ModelMediaTypeInputFormatter<T> : TextInputFormatter
        where T : class
    {
        private readonly Func<IFormatter<T>> _formatterFactory;

        public ModelMediaTypeInputFormatter(Func<IFormatter<T>> formatterFactory, string mediaType)
        {
            _formatterFactory = formatterFactory;
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

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            var result = await _formatterFactory().Read(context.HttpContext.Request.Body);
            return await InputFormatterResult.SuccessAsync(result);
        }
    }
}