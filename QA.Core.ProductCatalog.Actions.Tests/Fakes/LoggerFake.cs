using System;

namespace QA.Core.ProductCatalog.Actions.Tests.Fakes
{
	internal class LoggerFake : ILogger
	{
		public Exception LastException { get; set; }

		#region ILogger implementation
		public void ErrorException(string message, Exception exception, params object[] parameters)
		{
			LastException = exception;
		}

		public void Info(string message, params object[] parameters)
		{
		}

		public void Info(Func<string, string> message, params object[] parameters)
		{
		}

		public void Debug(string message, params object[] parameters)
		{
		}

		public void Debug(Func<string, string> message, params object[] parameters)
		{
		}

		public void Fatal(string message, Exception exception, params object[] parameters)
		{
		}

		public void Error(Func<string, string> message, params object[] parameters)
		{
		}

		public void Error(string message, params object[] parameters)
		{
		}

		public void Fatal(string message, params object[] parameters)
		{

		}

		public void Fatal(Func<string, string> message, params object[] parameters)
		{
		}

		public void Dispose()
		{
		}
		#endregion
	}
}
