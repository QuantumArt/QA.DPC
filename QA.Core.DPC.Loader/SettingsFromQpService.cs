using QA.Core.Cache;
using QA.Core.DPC.QP.Services;
using Quantumart.QP8.BLL.Services.API;
using System;
using QA.Core.DPC.QP.Cache;
using QA.ProductCatalog.ContentProviders;

namespace QA.Core.DPC.Loader
{
    public class SettingsFromQpService : SettingsServiceBase
	{
		private readonly DbService _dbService;
		private readonly IVersionedCacheProvider _cacheProvider;

		private readonly TimeSpan _cacheTimeSpan = TimeSpan.FromMinutes(5);

		public SettingsFromQpService(IVersionedCacheProvider cacheProvider, IConnectionProvider connectionProvider)
            : base(connectionProvider, cacheProvider)
		{

            _dbService = new DbService(_customer.ConnectionString, 1);
			_cacheProvider = cacheProvider;
		}

		public override string GetSetting(string title)
		{
			const string key = "AllQpSettings";

			var allSettings = _cacheProvider.GetOrAdd(key, new[] { CacheTags.QP8.DB }, _cacheTimeSpan, _dbService.GetAppSettings);

			return allSettings.ContainsKey(title) ? allSettings[title] : null;
		}
	}
}
