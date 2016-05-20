using System;
using System.Configuration;
using System.Linq;

namespace QA.ProductCatalog.StressTestUtility
{
	public static class Configuration
	{
		public static TimeSpan UpdateDelay
		{
			get { return GetTimeSpanValue("UpdateDelay", TimeSpan.FromSeconds(100)); }
		}

		public static TimeSpan PublishDelay
		{
			get { return GetTimeSpanValue("UpdateDelay", TimeSpan.FromSeconds(100)); }
		}

		public static TimeSpan SimplePublishDelay
		{
			get { return GetTimeSpanValue("SimplePublishDelay", TimeSpan.FromSeconds(100)); }
		}

		public static int[] UpdateIds
		{
			get	{ return GetIds("UpdateIds"); }
		}

		public static int[] PublishIds
		{
			get { return GetIds("PublishIds"); }
		}

		public static int UserId
		{
			get { return GetIntValue("UserId", 1); }
		}

		public static int UpdateThreadsCount
		{
			get { return GetIntValue("UpdateThreadsCount", 1); }
		}

		public static int PublishThreadsCount
		{
			get { return GetIntValue("PublishThreadsCount", 1); }
		}

		public static int SimplePublishThreadsCount
		{
			get { return GetIntValue("SimplePublishThreadsCount", 1); }
		}

		public static int StatusTypeId
		{
			get { return GetIntValue("StatusTypeId", 0); }
		}

		private static int GetIntValue(string key, int defaultValue)
		{
			int value;

			if (!int.TryParse(ConfigurationManager.AppSettings[key], out value))
			{
				value = defaultValue;
			}

			return value;
		}

		private static TimeSpan GetTimeSpanValue(string key, TimeSpan defaultValue)
		{
			TimeSpan value;

			if (!TimeSpan.TryParse(ConfigurationManager.AppSettings[key], out value))
			{
				value = defaultValue;
			}

			return value;
		}

		private static int[] GetIds(string key)
		{
			string data = ConfigurationManager.AppSettings[key];
			string[] values = data.Split(new[] { ',' });
			return values.Select(id => int.Parse(id)).ToArray();
		}
	}
}
