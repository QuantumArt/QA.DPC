﻿using QA.Core.Cache;
using QA.Core.DPC.QP.Servives;
using Quantumart.QP8.BLL.Services.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.DPC.Loader
{
	public class SettingsFromQpService : SettingsServiceBase
	{
		private readonly DbService _dbService;
		private readonly ICacheProvider _cacheProvider;

		private readonly TimeSpan _cacheTimeSpan = TimeSpan.FromMinutes(5);

		public SettingsFromQpService(ICacheProvider cacheProvider, IConnectionProvider connectionProvider)
            : base(connectionProvider)
		{

            _dbService = new DbService(_connectionString, 1);
			_cacheProvider = cacheProvider;
		}

		public override string GetSetting(string title)
		{
			const string key = "AllQpSettings";

			var allSettings = _cacheProvider.GetOrAdd(key, _cacheTimeSpan, _dbService.GetAppSettings);

			return allSettings.ContainsKey(title) ? allSettings[title] : null;
		}
	}
}
