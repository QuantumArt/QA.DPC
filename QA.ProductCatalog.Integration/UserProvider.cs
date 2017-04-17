using System.Configuration;
using QA.ProductCatalog.Infrastructure;
using System.Web;
using QA.Core.Web;
using System.Data;
using Quantumart.QPublishing;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.OnScreen;
using QA.Core.DPC.QP.Servives;

namespace QA.ProductCatalog.Integration
{
    public class UserProvider : IUserProvider
    {
		private const string QPUserIdKey = "backend_userid";
		private const string QPSIdKey = "backend_sid";
		private const string QPActionCodeKey = "actionCode";
		private const string QPItemIdKey = "content_item_id";
        static readonly RequestLocal<int> _forcedUserId = new RequestLocal<int>();
        private readonly string _connectionString;

        public static int ForcedUserId
        {
            set
            {
                _forcedUserId.Value = value;
            }
	        get { return _forcedUserId.Value; }
        }



        public UserProvider(IConnectionProvider connectionProvider)
        {
            _connectionString = connectionProvider.GetConnection();
        }

        public int GetUserId()
        {
            if (_forcedUserId.Value > 0)
                return _forcedUserId.Value;

			if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
				var useridObj = HttpContext.Current.Session[QPUserIdKey];
				int userid = 0;
				string sidKey = string.Format("{0}-{1}-{2}", QPSIdKey, HttpContext.Current.Request.QueryString[QPActionCodeKey], HttpContext.Current.Request.QueryString[QPItemIdKey]);
				string sid = HttpContext.Current.Session[sidKey] as string;
				string newSid = HttpContext.Current.Request.QueryString[QPSIdKey];

				if (!string.IsNullOrEmpty(newSid))
				{
					HttpContext.Current.Session[sidKey] = newSid;
				}
               
				if (useridObj != null)
                {
					userid = (int)useridObj;
                }

				if (!string.IsNullOrEmpty(newSid) && newSid != sid || userid == 0)
                {
					userid = QScreen.AuthenticateForCustomTab();
					HttpContext.Current.Session[QPUserIdKey] = userid;
                }

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