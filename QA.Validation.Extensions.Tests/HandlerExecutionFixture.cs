using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using Moq;

namespace QA.Validation.Extensions.Tests
{
    /// <summary>
    /// Настройка среды исполнения веб-приложения (настройка тестового объекта HttpContextBase).
    /// Для генерации и настройки объекта используется Moq
    /// </summary>
    public class HandlerExecutionFixture : IDisposable
    {
        private Mock<HttpRequestBase> _mockHttpRequest;
        private Mock<HttpResponseBase> _mockHttpResponse;
        private Mock<HttpContextBase> _mockHttpContext;
        private bool _isDisposed;
        private Mock<HttpSessionStateBase> _mockHttpSession;
        private Mock<HttpServerUtilityBase> _mockHttpServer;

        public static HandlerExecutionFixture Create(bool isStrict = false)
        {
            return new HandlerExecutionFixture(isStrict);
        }

        private HandlerExecutionFixture(bool isStrict)
        {
            var mode = isStrict ? MockBehavior.Strict : MockBehavior.Default;
            _mockHttpRequest = new Mock<HttpRequestBase>(mode);
            _mockHttpResponse = new Mock<HttpResponseBase>(mode);
            _mockHttpContext = new Mock<HttpContextBase>(mode);

            _mockHttpSession = new Mock<HttpSessionStateBase>(mode);
            _mockHttpServer = new Mock<HttpServerUtilityBase>(mode);

            _mockHttpContext.SetupGet(x => x.Request).Returns(_mockHttpRequest.Object);
            _mockHttpContext.SetupGet(x => x.Response).Returns(_mockHttpResponse.Object);
            _mockHttpContext.SetupGet(x => x.Server).Returns(_mockHttpServer.Object);
            _mockHttpContext.SetupGet(x => x.Session).Returns(_mockHttpSession.Object);

            _mockHttpRequest.Setup(x => x.ValidateInput()).Callback(() => { });
            _mockHttpSession.Setup(x => x[It.IsAny<string>()]).Returns<object>(null);
        }

        /// <summary>
        /// Установка HttpMethod
        /// </summary>
        /// <returns>self</returns>
        public HandlerExecutionFixture WithHttpMethod(string method)
        {
            _mockHttpRequest.SetupGet(x => x.HttpMethod).Returns(method);
            return this;
        }

        /// <summary>
        /// Установка ContentType
        /// </summary>
        /// <returns>self</returns>
        public HandlerExecutionFixture WithContentType(string contentType)
        {
            _mockHttpRequest.SetupGet(x => x.ContentType).Returns(contentType);
            return this;
        }

        /// <summary>
        /// Установка InputStream для post-запроса
        /// </summary>
        /// <returns>self</returns>
        public HandlerExecutionFixture WithInputStream(Stream stream)
        {
            _mockHttpRequest.SetupGet(x => x.InputStream).Returns(stream);
            return this;
        }
        /// <summary>
        /// Установка InputStream для post-запроса (устанавливается текст)
        /// </summary>
        /// <returns>self</returns>
        public HandlerExecutionFixture WithInputStream(string text)
        {
            var stream = new MemoryStream(new UTF8Encoding().GetBytes(text));
            _mockHttpRequest.SetupGet(x => x.InputStream).Returns(stream);
            return this;
        }

        public HandlerExecutionFixture WithOutputStream(StringWriter writer)
        {
            _mockHttpResponse.SetupGet(x => x.Output).Returns(writer);
            return this;
        }

        public HandlerExecutionFixture WithResponseHeaders(NameValueCollection headers)
        {
            _mockHttpResponse.SetupGet(x => x.Headers).Returns(headers);
            return this;
        }

        public HandlerExecutionFixture WithRequestHeaders(NameValueCollection headers)
        {
            _mockHttpRequest.SetupGet(x => x.Headers).Returns(headers);
            return this;
        }

        public HandlerExecutionFixture WithForm(NameValueCollection form)
        {
            _mockHttpRequest.SetupGet(x => x.Form).Returns(form);
            return this;
        }

        public HandlerExecutionFixture WithQueryString(NameValueCollection queryString)
        {
            _mockHttpRequest.SetupGet(x => x.QueryString).Returns(queryString);
            return this;
        }


        public HandlerExecutionFixture AllowSetResponseContentType()
        {
            _mockHttpResponse.SetupSet(x => x.ContentType = It.IsAny<string>())
                .Callback<string>(val => { });
            return this;
        }

        /// <summary>
        /// Установка собственного буфер для Response.Write(..)
        /// </summary>
        /// <returns>self</returns>
        public HandlerExecutionFixture WithWriteMethod(StringWriter writer)
        {
            _mockHttpResponse.Setup(x => x.Write(It.IsAny<string>()))
                .Callback<string>(y => writer.Write(y));
            return this;
        }

        /// <summary>
        /// Устанавливает Request.Url
        /// </summary>
        /// <returns>self</returns>
        public HandlerExecutionFixture WithUrl(Uri url)
        {
            _mockHttpRequest.SetupGet(x => x.Url)
                .Returns(() => url);

            return this;
        }

        /// <summary>
        /// Устанавливает Request.Url
        /// </summary>
        /// <returns>self</returns>
        public HandlerExecutionFixture WithPathInfo(string pathInfo)
        {
            _mockHttpRequest.SetupGet(x => x.PathInfo)
                .Returns(() => pathInfo);

            return this;
        }

        /// <summary>
        /// Устанавливает Request.AppRelativeCurrentExecutionFilePath (используется в mvc для определения контроллера)
        /// </summary>
        /// <returns>self</returns>
        public HandlerExecutionFixture WithAppRelativeCurrentExecutionFilePath(string path)
        {
            _mockHttpRequest.SetupGet(x => x.AppRelativeCurrentExecutionFilePath)
                .Returns(() => path);

            return this;
        }

        /// <summary>
        /// Получить объект HttpContextBase.
        /// Данный метод можно вызывать только один раз
        /// </summary>
        /// <returns>экземпляр настроенного HttpContextBase</returns>
        public HttpContextBase Use()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("HandlerExecutionFixture");
            }

            Dispose();

            return _mockHttpContext.Object;
        }

        #region IDisposable Members

        public void Dispose()
        {
            _isDisposed = true;
        }

        #endregion
    }
}
