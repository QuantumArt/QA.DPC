using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using QA.Core.Cache;
using QA.Core.DPC.QP.Cache;
using QA.Core.DPC.QP.Services;
using Quantumart.QPublishing.Database;

namespace QA.ProductCatalog.ContentProviders
{
	public class SettingsFromQpCoreService : SettingsServiceBase
	{
		private readonly IVersionedCacheProvider2 _cacheProvider;

		private readonly TimeSpan _cacheTimeSpan = TimeSpan.FromMinutes(5);

		public SettingsFromQpCoreService(IVersionedCacheProvider2 cacheProvider, IConnectionProvider connectionProvider)
            : base(connectionProvider, cacheProvider)
		{

			_cacheProvider = cacheProvider;
		}

		public override string GetSetting(string title)
		{
			const string key = "AllQpSettings";

			var allSettings = _cacheProvider.GetOrAdd(key, new[] { CacheTags.QP8.DB }, _cacheTimeSpan, GetAppSettings, true, CacheItemPriority.NeverRemove);

			return allSettings.ContainsKey(title) ? allSettings[title] : null;
		}

		private Dictionary<string, string> GetAppSettings()
		{
			var cnn = new DBConnector(_customer.ConnectionString, _customer.DatabaseType);
			var query = "select * from app_settings";
			return cnn.GetRealData(query).AsEnumerable()
				.ToDictionary(n => n["key"].ToString(), m => m["value"].ToString()
				);
		}
	}
}
