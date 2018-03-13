using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QPublishing.Database;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http.Routing;

namespace QA.ProductCatalog.WebApi.App_Start
{
    public class IdentityResolver
    {
        private const int DefaultUserId = 1;
        private const string QueryTemplate = @"
            select
	            [authorization].[QP User] [UserId]
            from
	            content_{0} [authorization]
	            join content_{1} [tokens] on [authorization].[Token User] = [tokens].[CONTENT_ITEM_ID]
	            join item_to_item [link] on [authorization].[CONTENT_ITEM_ID] = [link].[l_item_id] and [authorization].[{3}] = [link].[link_id]
	            join content_{2} [services] on link.[r_item_id] = [services].[CONTENT_ITEM_ID]
            where
	            (@token is null and [tokens].[Name] = 'Default' or [tokens].[AccessToken] = @token) and
	            [services].[Slug] = @slug and
	            [services].[Version] = @version";

        private readonly IIdentityProvider _identityProvider;
        private readonly IConnectionProvider _connectionProvider;
        private readonly ISettingsService _settingsService;

        public IdentityResolver(IIdentityProvider identityProvider, IConnectionProvider connectionProvider, ISettingsService settingsService)
        {
            _identityProvider = identityProvider;
            _connectionProvider = connectionProvider;
            _settingsService = settingsService;
        }

        public void ResolveIdentity(HttpRequest httpRequest)
        {
            var subroutes = ((IHttpRouteData[])httpRequest.RequestContext.RouteData.Values["MS_SubRoutes"]).FirstOrDefault();
            var customerCode = subroutes.Values["customerCode"] as string;

            Identity identity = null;            

            if (UseAuthorization())
            {                
                var method = httpRequest.HttpMethod;
                var token = httpRequest.Headers["X-Auth-Token"];
                var slug = subroutes.Values["slug"] as string;
                var version = subroutes.Values["version"] as string;

                var userId = GetUserId(slug, version, token, method);

                if (userId.HasValue)
                {
                    identity = new Identity(customerCode, userId.Value, true);
                }
                else
                {
                    identity = new Identity(customerCode, 0, false);
                }
            }
            else
            {
                identity = new Identity(customerCode, GetDefaultUserId(), true);
            }

            _identityProvider.Identity = identity;
        }

        private bool UseAuthorization()
        {
            if (bool.TryParse(ConfigurationManager.AppSettings["UseAuthorization"], out bool useAuthorization))
            {
                return useAuthorization;
            }
            else
            {
                return false;
            }
        }

        private string GetQuery(string method)
        {            
            int authorizationCoontentId = GetContentId(SettingsTitles.API_AUTHORIZATION_CONTENT_ID);
            int tokensCoontentId = GetContentId(SettingsTitles.HIGHLOAD_API_USERS_CONTENT_ID);
            int servicesCoontentId = GetContentId(SettingsTitles.PRODUCT_SERVICES_CONTENT_ID);
            string field = method == "GET" ? "Read Service" : "Write Service";

            return string.Format(QueryTemplate, authorizationCoontentId, tokensCoontentId, servicesCoontentId, field);
        }

        private int GetContentId(SettingsTitles key)
        {
            if (int.TryParse(_settingsService.GetSetting(key), out int contentId))
            {
                return contentId;
            }
            else
            {
                throw new Exception($"Setting {key} must be a valid content id");
            }
        }

        private int? GetUserId(string slug, string version, string token, string method)
        {
            var connection = _connectionProvider.GetConnection();
            var dbConnector = new DBConnector(connection);
            var query = GetQuery(method);
            var sqlCommand = new SqlCommand(query);

            sqlCommand.Parameters.AddWithValue("@slug", slug);
            sqlCommand.Parameters.AddWithValue("@version", version);
            sqlCommand.Parameters.AddWithValue("@token", (object)token ?? DBNull.Value);

            var dt = dbConnector.GetRealData(sqlCommand);
            var userIds = dt.AsEnumerable().Select(row => row["UserId"] == DBNull.Value ? null : (int?)(decimal?)row["UserId"]).ToArray();

            if (userIds.Any())
            {
                return userIds.First() ?? GetDefaultUserId();
            }
            else
            {
                return null;
            }
        }

        private int GetDefaultUserId()
        {
            if (int.TryParse(ConfigurationManager.AppSettings["UserId"], out int userId))
            {
                return userId;
            }
            else
            {
                return DefaultUserId;
            }
        }    
    }
}