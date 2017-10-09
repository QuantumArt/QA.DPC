using System.Configuration;
using QA.Core.Logger;

namespace QA.Core.DPC.API
{
	public class ProxyConfiguration : IProxyConfiguration
	{
		public string Host { get; private set; }

		public ProxyConfiguration(ILogger logger)
		{
			Host = ConfigurationManager.AppSettings["ServiceProxy.Host"];
			logger.LogDebug(() => GetType().Name + ": " + ToString());
		}

		public override string ToString()
		{
			return new { Host }.ToString();;
		}
	}
}
