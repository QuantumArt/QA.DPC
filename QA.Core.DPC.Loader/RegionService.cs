using QA.Core.Cache;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.ContentProviders;

namespace QA.Core.DPC.Loader
{
    public class RegionService : IRegionService
    {
        #region Глобальные переменные
        private readonly IVersionedCacheProvider _cacheProvider;
        private readonly ISettingsService _settingsService;
        private readonly TimeSpan _cachePeriod = new TimeSpan(0, 10, 0);
        private readonly string _connectionString;

        #endregion

        #region Конструкторы
        public RegionService(IVersionedCacheProvider cacheProvider, ISettingsService settingsService, IConnectionProvider connectionProvider)
        {
            _cacheProvider = cacheProvider;
            _settingsService = settingsService;
            _connectionString = connectionProvider.GetConnection();
        }
        #endregion

        class Region
        {
            public int Id { get; set; }
            public int ParentId { get; set; }
            public string Title { get; set; }
        }

        private Dictionary<int, Region> LoadRegions() 
        {
            
            var regContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.REGIONS_CONTENT_ID));
            
            var regions = new List<Region>();
            
            var sql = $@"select content_item_id, parent, Title from content_{regContentId}_united where ARCHIVE = 0";

            using (var cs = new QPConnectionScope(_connectionString))
            {
                var con = cs.DbConnection;

                if (con.State != ConnectionState.Open)
                    con.Open();
                
                using (var cmd = new SqlCommand(sql, con))
                {

                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            var regionId = (int)rd.GetDecimal(0);
                            var parentId = rd.IsDBNull(1) ? 0 : (int)rd.GetDecimal(1);
                            var title = rd.GetString(2);
                            regions.Add(new Region { Id = regionId, ParentId = parentId, Title = title});
                        }
                        rd.Close();
                    }

                }
            }

            return regions.ToDictionary(n => n.Id, m => m);
        }

        private Dictionary<int, Region> GetRegions()
        {
            const string key = "RegionService__Regions";
            var tags = new[] {_settingsService.GetSetting(SettingsTitles.REGIONS_CONTENT_ID)};
            return _cacheProvider.GetOrAdd(key, tags, _cachePeriod, LoadRegions);          
        }

        #region IRegionService
        public List<int> GetParentsIds(int id)
        {
            var result = new List<int>();
            var regions = GetRegions();
            while (true)
            {
                var region = regions[id];
                if (region.ParentId == 0) break;
                id = region.ParentId;
                result.Add(id);
            }

            return result;

        }
        #endregion

    }
}
