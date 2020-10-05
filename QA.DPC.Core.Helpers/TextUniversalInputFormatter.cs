using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;


namespace QA.DPC.Core.Helpers
{
    internal class MediaTypeHeaderValues
    {
        public static readonly MediaTypeHeaderValue ApplicationJson
            = MediaTypeHeaderValue.Parse("application/json").CopyAsReadOnly();

        public static readonly MediaTypeHeaderValue TextPlain
            = MediaTypeHeaderValue.Parse("text/plain").CopyAsReadOnly();

        public static readonly MediaTypeHeaderValue ApplicationXml
            = MediaTypeHeaderValue.Parse("application/xml").CopyAsReadOnly();

        public static readonly MediaTypeHeaderValue TextXml
            = MediaTypeHeaderValue.Parse("text/xml").CopyAsReadOnly();
    }

    public class TextUniversalInputFormatter : TextInputFormatter
    {


        public TextUniversalInputFormatter()
        {
            SupportedEncodings.Add(UTF8EncodingWithoutBOM);
            SupportedEncodings.Add(UTF16EncodingLittleEndian);
            SupportedEncodings.Add(Encoding.UTF8);

            SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationJson);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationXml);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.TextPlain);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.TextXml);
        }

        protected override bool CanReadType(Type type)
        {
            return true;
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(
            InputFormatterContext context,
            Encoding encoding)
        {
            string data = null;
            using (var streamReader = new StreamReader(context.HttpContext.Request.Body, encoding))
            {
                data = await streamReader.ReadToEndAsync();
            }

            return await InputFormatterResult.SuccessAsync(data);
        }
    }
}
