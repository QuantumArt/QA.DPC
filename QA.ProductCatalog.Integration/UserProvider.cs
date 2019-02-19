using System;
using QA.ProductCatalog.Infrastructure;
using QA.Core.Web;
using System.Data;
using System.Threading;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.ContentProviders;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.OnScreen;
#if !NETSTANDARD
using System.Web;
#else
using Microsoft.AspNetCore.Http;
#endif

namespace QA.ProductCatalog.Integration
{
    public class UserProvider : IUserProvider
    {
		private const string QPUserIdKey = "backend_userid";
		private const string QPSIdKey = "backend_sid";
		private const string QPActionCodeKey = "actionCode";
		private const string QPItemIdKey = "content_item_id";
        static readonly ThreadLocal<int> _forcedUserId = new ThreadLocal<int>();
        private readonly string _connectionString;
#if NETSTANDARD        
        private IHttpContextAccessor _accessor;
#endif

        public static int ForcedUserId
        {
            set
            {
                _forcedUserId.Value = value;
            }
	        get { return _forcedUserId.Value; }
        }
        
	    public HttpContext HttpContext
	    {
		    get
		    {
#if !NETSTANDARD
			    return HttpContext.Current;
#else
				return _accessor.HttpContext;
#endif
		    }

	    }


        public UserProvider(IConnectionProvider connectionProvider)
        {
            _connectionString = connectionProvider.GetConnection();
        }
        
#if NETSTANDARD        
        public UserProvider(IConnectionProvider connectionProvider, IHttpContextAccessor accessor)
        {
	        _connectionString = connectionProvider.GetConnection();
	        _accessor = accessor;
        }
#endif


        public int GetUserId()
        {
            if (_forcedUserId.Value > 0)
                return _forcedUserId.Value;

			if (HttpContext != null && HttpContext.Session != null)
            {
#if !NETSTANDARD	            
				var userid = (int)(HttpContext.Session[QPUserIdKey] ?? 0);
				string sidKey = string.Format("{0}-{1}-{2}", QPSIdKey, HttpContext.Request.QueryString[QPActionCodeKey], HttpContext.Request.QueryString[QPItemIdKey]);
				string sid = HttpContext.Session[sidKey] as string;
				string newSid = HttpContext.Request.QueryString[QPSIdKey];

				if (!string.IsNullOrEmpty(newSid))
				{
					HttpContext.Session[sidKey] = newSid;
				}

#else
				var userid = HttpContext.Session.GetInt32(QPUserIdKey) ?? 0;
				string sidKey =
					$"{QPSIdKey}-{HttpContext.Request.Query[QPActionCodeKey]}-{HttpContext.Request.Query[QPItemIdKey]}";
				string sid = HttpContext.Session.GetString(sidKey);
				string newSid = HttpContext.Request.Query[QPSIdKey];

				if (!string.IsNullOrEmpty(newSid))
				{
					HttpContext.Session.SetString(sidKey, newSid);
				}
#endif
               
#if !NETSTANDARD
				if (!string.IsNullOrEmpty(newSid) && newSid != sid || userid == 0)
				{
					var backendSid = (HttpContext.Request["backend_sid"] ?? String.Empty).Replace("'", "''");	     
	                userid = new QScreen(new DBConnector(_connectionString)).AuthenticateForCustomTab(backendSid);
					HttpContext.Session[QPUserIdKey] = userid;
           
                }
#else
	            if (!string.IsNullOrEmpty(newSid) && newSid != sid || userid == 0)
	            {
		            var backendSid = (HttpContext.Request.Query["backend_sid"]).ToString().Replace("'", "''");	     
		            userid = new QScreen(new DBConnector(_connectionString)).AuthenticateForCustomTab(backendSid);
		            HttpContext.Session.SetInt32(QPUserIdKey, userid);
	            }      
#endif

				if (userid != 0)
                {
					_forcedUserId.Value = userid;
                }
				return userid;
            }
            return 0;
        }
	    
        public string GetUserName()
        {
            int userId = GetUserId();

            var dBConnector = new DBConnector(_connectionString);

            string str = string.Concat("select [FIRST_NAME], [LAST_NAME] from users where user_id = ", userId);
            
            DataTable cachedData = dBConnector.GetCachedData(str);

            if (cachedData == null || cachedData.Rows.Count == 0)
                return null;

            return cachedData.Rows[0]["FIRST_NAME"] + " " + cachedData.Rows[0]["LAST_NAME"];
        }
    }
}