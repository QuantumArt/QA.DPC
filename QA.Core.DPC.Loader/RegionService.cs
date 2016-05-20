

using QA.Core.Cache;
using QA.Core.DPC.Loader.Resources;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Data;
using QA.Core.DPC.Loader.Services;
using QA.Core.ProductCatalog.Actions.Services;

namespace QA.Core.DPC.Loader
{
    public class RegionService : IRegionService
    {
        #region Константы
        private const string KEY_CONNTECTION_STRING = "qp_database";
        #endregion

        #region Глобальные переменные
        private IVersionedCacheProvider _cacheProvider;
        private ICacheItemWatcher _cacheItemWatcher;
        private ISettingsService _settingsService;

        //private IUserProvider _userProvider;

        private static TimeSpan _cachePeriod = new TimeSpan(0, 10, 0);
#warning Настройки времени кэширования вынести в конфиг //TODO: получать время кэширования из конфига
        private readonly string _connectionString;
        private readonly IFieldService _fieldService;
        private readonly IArticleService _articleService;
        #endregion

        #region Конструкторы
        public RegionService(IVersionedCacheProvider cacheProvider, ICacheItemWatcher cacheItemWatcher, IUserProvider userProvider, ISettingsService settingsService)
        {
            _cacheProvider = cacheProvider;
            _cacheItemWatcher = cacheItemWatcher;
            _settingsService = settingsService;


            //_userProvider = userProvider;
            this._cacheItemWatcher.TrackChanges();
            var connectinStringObject = ConfigurationManager.ConnectionStrings[KEY_CONNTECTION_STRING];
            if (connectinStringObject == null)
            {
                throw new Exception(string.Format(ProductLoaderResources.ERR_CONNECTION_STRING_NO_EXISTS, KEY_CONNTECTION_STRING));
            }
            _connectionString = connectinStringObject.ConnectionString;
        }
        #endregion


        #region IRegionService
        public List<int> GetParentsIds(int id)
        {
            var regContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.REGIONS_CONTENT_ID));

            var key = string.Format("GetParentsIds_{0}", id);
            return _cacheProvider.GetOrAdd(key, new string[] { regContentId.ToString() }, _cachePeriod, () =>
            {
                var sql = string.Format(@"with region_tree as (
                       select content_item_id, parent, Title, ARCHIVE
                       from content_{0}
                       where content_item_id = (select parent from content_{0} where content_item_id = @id and ARCHIVE = 0) and ARCHIVE = 0
                       union all
                       select c.content_item_id, c.parent, c.Title, c.ARCHIVE
                       from content_{0} c
                         join region_tree p on p.parent = c.content_item_id  and c.ARCHIVE = 0
                    ) 
                    select *
                    from region_tree;", regContentId, id);

                List<int> ids = new List<int>();
                using (var cs = new QPConnectionScope(_connectionString))
                {
                    var con = cs.DbConnection;

                    if (con.State != System.Data.ConnectionState.Open)
                        con.Open();

                    using (SqlCommand cmd = new SqlCommand(
                       sql, con))
                    {
                        SqlParameter idParametr = new SqlParameter();
                        idParametr.ParameterName = "@id";// Defining Name
                        idParametr.SqlDbType = SqlDbType.Int; // Defining DataType
                        idParametr.Direction = ParameterDirection.Input; // Setting the direction 
                        idParametr.Value = id;

                        cmd.Parameters.Add(idParametr);

                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                ids.Add((int)rd.GetDecimal(0));
                            }
                            rd.Close();
                        }

                    }

                }

                return ids;
            });
        }
        #endregion

        #region Закрытые методы
        #endregion



    }
}
