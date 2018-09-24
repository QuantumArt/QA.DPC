using System;
using Microsoft.Extensions.Caching.Memory;
using QA.Core.DPC.Loader;
using QA.Core.DPC.QP.Services;
using Quantumart.QP8.BLL.Services.API;


namespace QA.DPC.Core.Helpers
{
	public class SettingsFromQpCoreService : SettingsServiceBase
	{
		private readonly DbService _dbService;
		private readonly IVersionedCacheProvider2 _cacheProvider;

		private readonly TimeSpan _cacheTimeSpan = TimeSpan.FromMinutes(5);

		public SettingsFromQpCoreService(IVersionedCacheProvider2 cacheProvider, IConnectionProvider connectionProvider)
            : base(connectionProvider)
		{

            _dbService = new DbService(_connectionString, 1);
			_cacheProvider = cacheProvider;
		}

		public override string GetSetting(string title)
		{
			const string key = "AllQpSettings";

			var allSettings = _cacheProvider.GetOrAdd(key, new[] { CacheTags.QP8.DB }, _cacheTimeSpan, _dbService.GetAppSettings, true, CacheItemPriority.NeverRemove);

			return allSettings.ContainsKey(title) ? allSettings[title] : null;
		}
	}
}
