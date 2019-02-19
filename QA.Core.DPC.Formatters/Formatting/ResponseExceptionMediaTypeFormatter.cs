#if !NETSTANDARD
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace QA.Core.DPC.Formatters.Formatting
{
	public class ResponseExceptionMediaTypeFormatter : MediaTypeFormatter
	{
		private readonly HttpStatusCode _statusCode;

		public ResponseExceptionMediaTypeFormatter(HttpStatusCode statusCode, params string[] mediaTypes)
		{
			_statusCode = statusCode;

			foreach (string mediaType in mediaTypes)
			{
				var mediaTypeHeaderValue = new MediaTypeHeaderValue(mediaType);
				SupportedMediaTypes.Add(mediaTypeHeaderValue);
			}
		}

		public override bool CanReadType(Type type)
		{
			return true;
		}

		public override bool CanWriteType(Type type)
		{
			return type != typeof(HttpError);
		}

		public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
		{
			throw new HttpResponseException(_statusCode);
		}

		public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
		{
			throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
		}
	}
}
#endif