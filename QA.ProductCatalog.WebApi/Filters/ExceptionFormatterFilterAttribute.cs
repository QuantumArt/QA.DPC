//using System;
//using System.Linq;
//using System.Net.Http.Formatting;
//using System.Net.Http.Headers;
//using Microsoft.AspNetCore.Mvc.Filters;
//
//
//namespace QA.ProductCatalog.WebApi.Filters
//{
//	public class ExceptionFormatterFilterAttribute : ExceptionFilterAttribute
//	{
//		private readonly MediaTypeFormatterCollection _formatters;
//
//		public ExceptionFormatterFilterAttribute(MediaTypeFormatterCollection formatters)
//		{
//			_formatters = formatters;
//		}
//
//		public override void OnException(ExceptionContext context)
//		{
//			foreach (var formatter in _formatters)
//			{
//				var mediaType = GetMediaType(formatter, actionExecutedContext);
//
//				if (mediaType != null)
//				{
//					var message = new HttpResponseMessage(HttpStatusCode.InternalServerError)
//					{
//						ReasonPhrase = actionExecutedContext.Exception.Message,
//						Content = new ObjectContent<Exception>(actionExecutedContext.Exception, formatter, mediaType)
//					};
//
//					throw new HttpResponseException(message);
//				}
//			}
//		}
//
//		private MediaTypeHeaderValue GetMediaType(MediaTypeFormatter formatter, ExceptionContext context)
//		{
//			if (formatter.CanWriteType(typeof(Exception)))
//			{
//				var MatchedMediaTypes = from mapping in formatter.MediaTypeMappings
//										let matchFactor = mapping.TryMatchMediaType(context.HttpContext.Request)
//										where matchFactor > 0
//										orderby matchFactor descending
//										select mapping.MediaType;
//
//				return MatchedMediaTypes.FirstOrDefault();
//			}
//			else
//			{
//				return null;
//			}
//		}
//	}
//}