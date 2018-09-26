using System;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.QP.Services;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API;

namespace QA.DPC.Core.Helpers
{

    public class SettingsFromContentCoreService : SettingsServiceBase
    {
        private readonly IVersionedCacheProvider2 _cacheProvider;

        private readonly IReadOnlyArticleService _articleService;
        
        private readonly int _settingsContentId;

        public SettingsFromContentCoreService(IVersionedCacheProvider2 cacheProvider, 
            IConnectionProvider connectionProvider, 
            int settingsContentId)
            : base(connectionProvider)
        {
            _cacheProvider = cacheProvider;
            _settingsContentId = settingsContentId;
        }


        private const string FIELD_NAME_TITLE = "Title";

        private const string FIELD_NAME_VALUE = "Value";

        private readonly TimeSpan _cachePeriod = new TimeSpan(3, 10, 0);

        public override string GetSetting(string title)
        {
            var key = string.Format("GetSetting_{0}", title);
            return _cacheProvider.GetOrAdd(key, new[] {_settingsContentId.ToString()}, _cachePeriod,
                () => GetSettingValue(title), true, CacheItemPriority.NeverRemove);
        }

        private string GetSettingValue(string title)
        {
            using (new QPConnectionScope(_connectionString))
            {
                var articleService = new ArticleService(_connectionString, 1);

                var list = articleService.List(_settingsContentId, null).Where(x => !x.Archived && x.Visible);

                var res = list
                    .Where(x => x.FieldValues.Any(a => a.Field.Name == FIELD_NAME_TITLE && a.Value == title))
                    .Select(x =>
                        x.FieldValues.Where(a => a.Field.Name == FIELD_NAME_VALUE).Select(a => a.Value)
                            .FirstOrDefault())
                    .FirstOrDefault();

                return res;
            }
        }
    }
}
