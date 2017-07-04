using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Formatters.Formatting
{
	public class ModelMediaTypeFormatter<T> : MediaTypeFormatter
		where T : class
	{
		private readonly Func<IFormatter<T>> _formatterFactory;

		public ModelMediaTypeFormatter(Func<IFormatter<T>> formatterFactory, string mediaType)
		{
            _formatterFactory = formatterFactory;
			SupportedMediaTypes.Add(new MediaTypeHeaderValue(mediaType));
		}

		public override bool CanReadType(Type type)
		{
			return type == typeof(T);
		}

		public override bool CanWriteType(Type type)
		{
			return type == typeof(T);
		}

		public override async Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
		{
			return await _formatterFactory().Read(readStream);
		}

		public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
		{
            var f = _formatterFactory();
            return f.Write(writeStream, (T)value);
        }

		public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
		{
			if (mediaType != null)
			{
				headers.ContentType = new MediaTypeHeaderValue(mediaType.MediaType) { CharSet = Encoding.UTF8.WebName };
			}
		}
	}
}