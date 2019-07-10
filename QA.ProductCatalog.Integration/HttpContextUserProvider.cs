using System.Threading;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.ContentProviders;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.OnScreen;
using Microsoft.AspNetCore.Http;
using QA.Core.DPC.QP.Models;

namespace QA.ProductCatalog.Integration
{
    public class HttpContextUserProvider : IUserProvider
    {
		private const string QpUserIdKey = "backend_userid";
		private const string QpSidKey = "backend_sid";
		private const string QpActionCodeKey = "actionCode";
		private const string QpItemIdKey = "content_item_id";
		
		private readonly Customer _customer;
		private readonly IHttpContextAccessor _accessor;
		
        private static readonly ThreadLocal<int> ForcedUserIdStorage = new ThreadLocal<int>();

        public static int ForcedUserId
        {
	        get => ForcedUserIdStorage.Value;
	        set => ForcedUserIdStorage.Value = value;
        }
        
        private HttpContext HttpContext => _accessor.HttpContext;
        
        public HttpContextUserProvider(IConnectionProvider connectionProvider, IHttpContextAccessor accessor)
        {
	        _customer = connectionProvider.GetCustomer();
	        _accessor = accessor;
        }


        public int GetUserId()
        {
            if (ForcedUserId > 0)
                return ForcedUserId;

			if (HttpContext != null && HttpContext.Session != null)
            {
				var userid = HttpContext.Session.GetInt32(QpUserIdKey) ?? 0;
				var sidKey =
					$"{QpSidKey}-{GetQueryParameter(QpActionCodeKey)}-{GetQueryParameter(QpItemIdKey)}";
				var sid = HttpContext.Session.GetString(sidKey);
				var newSid = GetQueryParameter(QpSidKey);

				if (!string.IsNullOrEmpty(newSid))
				{
					HttpContext.Session.SetString(sidKey, newSid);
				}
               
	            if (!string.IsNullOrEmpty(newSid) && newSid != sid || userid == 0)
	            {
		            var backendSid = newSid?.Replace("'", "''");	     
		            userid = new QScreen(new DBConnector(_customer.ConnectionString, _customer.DatabaseType))
			            .AuthenticateForCustomTab(backendSid);
		            HttpContext.Session.SetInt32(QpUserIdKey, userid);
	            }      

				if (userid != 0)
                {
					ForcedUserId = userid;
                }
				
				return userid;
            }
            return 0;
        }
        
        private string GetQueryParameter(string key)
        {
	        var request = HttpContext.Request;
	        var value = request.Query[key].ToString();
	        if (string.IsNullOrEmpty(value) && request.Method == "POST")
	        {
		        value = request.Form[key].ToString();
	        }
	        return value;
        }

	    
        public string GetUserName()
        {
            var userId = GetUserId();

            var dBConnector = new DBConnector(_customer.ConnectionString, _customer.DatabaseType);

            var str = string.Concat("select FIRST_NAME, LAST_NAME from users where user_id = ", userId);
            
            var cachedData = dBConnector.GetCachedData(str);

            if (cachedData == null || cachedData.Rows.Count == 0)
                return null;

            return cachedData.Rows[0]["FIRST_NAME"] + " " + cachedData.Rows[0]["LAST_NAME"];
        }
    }
}