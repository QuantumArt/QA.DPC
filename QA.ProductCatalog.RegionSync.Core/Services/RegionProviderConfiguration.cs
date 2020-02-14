using System.Configuration;
using NLog;
using NLog.Fluent;


namespace QA.ProductCatalog.RegionSync.Core.Services
{
    public class RegionProviderConfiguration : IRegionProviderConfiguration
	{
		private const string ConnectionStringKey = "{0}";
		private const string RegionContentIdKey = "RegionProvider.{0}.RegionContentId";
		private const string UserIdKey = "RegionProvider.UserId";

		public string ConnectionString { get; private set; }
		public int RegionContentId { get; private set; }
		public int UserId { get; private set; }

		private static ILogger Logger = LogManager.GetCurrentClassLogger();

		public RegionProviderConfiguration(string key)
		{
			ConnectionString = ConfigurationManager.ConnectionStrings[string.Format(ConnectionStringKey, key)].ConnectionString;
			RegionContentId = GetIntValue(string.Format(RegionContentIdKey, key));
			UserId = GetIntValue(UserIdKey);
			Logger.Debug(GetType().Name + ToString());
		}

		protected int GetIntValue(string key)
		{
			int value;

			if (!int.TryParse(ConfigurationManager.AppSettings[key], out value))
			{
				value = 0;
			}

			return value;
		}

		public sealed override string ToString()
		{
			return new { RegionContentId, UserId }.ToString();
		}
	}
}
