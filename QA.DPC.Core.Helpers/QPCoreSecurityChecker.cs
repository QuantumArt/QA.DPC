using System;
using System.ComponentModel;
using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using QA.Core;
using QA.Core.DPC.QP.Services;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.OnScreen;

namespace QA.DPC.Core.Helpers
{
    public class QPCoreSecurityChecker: ISecurityChecker
    {
    
        private enum QpLanguage : byte
        {
            [Description("ru-RU")]
            Default = 0,

            [Description("en-US")]
            English = 1,

            [Description("ru-RU")]
            Russian = 2
        }
        
        protected static readonly string AuthenticationKey = "QP_AuthenticationKey";
        
        protected static readonly string UserLanguageFieldName = "LANGUAGE_ID";
        
        public static readonly string UserLanguageKey = "QP_User_Language";

        private readonly IHttpContextAccessor _httpContextAccessor;
        
        private readonly IConnectionProvider _provider;

        private readonly ILogger _logger;

        public QPCoreSecurityChecker(IHttpContextAccessor contextAccessor, IConnectionProvider provider, ILoggerFactory factory)
        {
            _httpContextAccessor = contextAccessor;
            _provider = provider;
            _logger = factory.CreateLogger(GetType());
        }

        public virtual bool CheckAuthorization()
        {
            var httpContext = _httpContextAccessor.HttpContext;
                
            if (httpContext?.Session == null)
            {
                return false;
            }

            var lm = httpContext.Session.GetInt32(AuthenticationKey);
            if (lm.HasValue && lm == 1)
            {
                httpContext.Items[DBConnector.LastModifiedByKey] =
                    httpContext.Session.GetInt32(DBConnector.LastModifiedByKey);
                return true;
            }
            
            var dbSettings = new DbConnectorSettings();
            var cache = (IMemoryCache) new MemoryCache(new MemoryCacheOptions());
            var dBConnector = new DBConnector(_provider.GetConnection(), dbSettings, cache, _httpContextAccessor);
            int userId = new QScreen(dBConnector).AuthenticateForCustomTab();
            bool found = userId > 0;
            if (found)
            {
                try
                {
                    var userInfo = GetUserInfo(userId, dBConnector);

                    if (userInfo != null && userInfo.Rows.Count > 0)
                    {
                        var lang = userInfo.Rows[0][UserLanguageFieldName].ToString();
                        int langId = 0;
                        int.TryParse(lang, out langId);

                        string langName = ((QpLanguage) Enum.Parse(typeof(QpLanguage), langId.ToString()))
                            .GetDescription();

                        httpContext.Session.SetString(UserLanguageKey, langName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    httpContext.Session.SetString(UserLanguageKey, QpLanguage.Default.GetDescription());
                }

                httpContext.Session.SetInt32(AuthenticationKey, 1);

                httpContext.Session.SetInt32(DBConnector.LastModifiedByKey, userId);
                httpContext.Items[DBConnector.LastModifiedByKey] = userId;
            }

            return found;
        }
        

        private static DataTable GetUserInfo(int userId, DBConnector dBConnector)
        {
            return dBConnector.GetCachedData($"select * from users where user_id = {userId}");
        }
    }
}

