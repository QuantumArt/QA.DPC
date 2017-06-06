using System;
using System.Linq;
using QA.Core.Cache;
using Quantumart.QP8.BLL;
using QA.Core.DPC.Loader.Resources;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.QP.Services;

namespace QA.Core.DPC.Loader
{
	public class SettingsFromContentService : SettingsServiceBase
	{
		private readonly IVersionedCacheProvider _cacheProvider;

		private readonly IReadOnlyArticleService _articleService;

		public SettingsFromContentService(
			IVersionedCacheProvider cacheProvider, 
			ICacheItemWatcher cacheItemWatcher,
			IReadOnlyArticleService articleService,
            IConnectionProvider connectionProvider)
            : base(connectionProvider)
		{
			_cacheProvider = cacheProvider;
			
			_articleService = articleService;

			cacheItemWatcher.TrackChanges();
		}

		private const int SETTINGS_CONTENT_ID = 349; //TODO: Вынести в конфиг?

		private readonly TimeSpan _cachePeriod = new TimeSpan(3, 10, 0);

		public override string GetSetting(string title)
		{
			var key = string.Format("GetSetting_{0}", title);
			return _cacheProvider.GetOrAdd(key, new [] { SETTINGS_CONTENT_ID.ToString() }, _cachePeriod, () =>
			{
				if (QPConnectionScope.Current == null)
				{
					using (new QPConnectionScope(_connectionString))
					{
						return GetSettingValue(title);
					}
				}
				else
				{
					return GetSettingValue(title);
				}
			});
		}

		private const string FIELD_NAME_TITLE = "Title";

		private const string FIELD_NAME_VALUE = "Value";

		private string GetSettingValue(string title)
		{
			using (new QPConnectionScope(_connectionString))
			{
				var articleService = _articleService;
				
				var list = articleService.List(SETTINGS_CONTENT_ID, null).Where(x => !x.Archived && x.Visible);
				
				var res = list
					.Where(x => x.FieldValues.Any(a => a.Field.Name == FIELD_NAME_TITLE && a.Value == title))
				    .Select(x => x.FieldValues.Where(a => a.Field.Name == FIELD_NAME_VALUE).Select(a => a.Value).FirstOrDefault())
					.FirstOrDefault();
	
				return res;
			}
		}
	}
}
