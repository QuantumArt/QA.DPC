using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using QA.Validation.Xaml.Extensions.Rules;
using QA.Validation.Xaml.Extensions.Rules.Remote;

namespace QA.Validation.Extensions.Tests.Stubs
{
    public class StubWebInteractionManager : IWebInteractionManager
    {
        private Func<ValidationHandlerBase> _handlerFactory;
        #region IWebInteractionManager Members

        public StubWebInteractionManager(Func<ValidationHandlerBase> handlerFactory)
        {
            _handlerFactory = handlerFactory;
        }

        public string GetResponseBody(Uri url, string modelToSent, string HttpMethod, string ContentType, NameValueCollection headers, int Timeout)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                var fixture = HandlerExecutionFixture.Create()
                    .WithHttpMethod(HttpMethod)
                    .WithRequestHeaders(headers)
                    .WithContentType(ContentType)
                    .AllowSetResponseContentType()
                    .WithInputStream(modelToSent)
                    .WithUrl(url)
                    .WithWriteMethod(writer);

                var handler = _handlerFactory();

                handler.OnProcessRequest(fixture.Use());
            }

            return sb.ToString();
        }

        #endregion
    }
}
