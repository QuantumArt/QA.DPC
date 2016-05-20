using System;

namespace QA.Core.DPC.API
{
	[Serializable]
	public class ProxyExternalException : Exception
	{
		public ProxyExternalException(string message)
			: base(message)
		{
		}

		public ProxyExternalException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
