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
    public class IdentityResolver : IdentityResolverBase
    {
        private const int DefaultUserId = 1;
        private const string CommonQueryTemplate = @"
            select
	            [authorization].[QP User] [UserId]
            from
	            content_{0}_united [authorization]
	            join content_{1}_united [tokens] on [authorization].[Token User] = [tokens].[CONTENT_ITEM_ID]
            where
	            (@token is null and [tokens].[Name] = 'Default' or [tokens].[AccessToken] = @token) and
	            [authorization].[{2}] = 1 and
                [authorization].Visible = 1 and
	            [authorization].Archive = 0 and
	            [tokens].Visible = 1 and
	            [tokens].Archive = 0";

        private const string ServiceQueryTemplate = @"
            select
	            [authorization].[QP User] [UserId]
            from
	            content_{0}_united [authorization]
	            join content_{1}_united [tokens] on [authorization].[Token User] = [tokens].[CONTENT_ITEM_ID]
	            join item_to_item [link] on [authorization].[CONTENT_ITEM_ID] = [link].[l_item_id] and [authorization].[{3}] = [link].[link_id]
	            join content_{2}_united [services] on link.[r_item_id] = [services].[CONTENT_ITEM_ID]
            where
	            (@token is null and [tokens].[Name] = 'Default' or [tokens].[AccessToken] = @token) and
	            [services].[Slug] = @slug and
	            [services].[Version] = @version and
	            [authorization].Visible = 1 and
	            [authorization].Archive = 0 and
	            [tokens].Visible = 1 and
	            [tokens].Archive = 0 and
	            [services].Visible = 1 and
	            [services].Archive = 0";

        private readonly IConnectionProvider _connectionProvider;
        private readonly ISettingsService _settingsService;

        public IdentityResolver(IIdentityProvider identityProvider, IConnectionProvider connectionProvider, ISettingsService settingsService)
            : base(identityProvider)
        {
            _connectionProvider = connectionProvider;
            _settingsService = settingsService;
        }

        public override void ResolveIdentity(HttpRequest httpRequest)
        {
            var subroutes = ((IHttpRouteData[])httpRequest.RequestContext.RouteData.Values["MS_SubRoutes"]).FirstOrDefault();
            var customerCode = GetRoute(subroutes, "customerCode");

            Identity identity = null;            

            if (UseAuthorization())
            {                
                var method = httpRequest.HttpMethod;
                var token = httpRequest.Headers["X-Auth-Token"];
                var slug = GetRoute(subroutes, "slug");
                var version = GetRoute(subroutes, "version");

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

        private string GetServiceQuery(string method)
        {            
            int authorizationCoontentId = GetContentId(SettingsTitles.API_AUTHORIZATION_CONTENT_ID);
            int tokensCoontentId = GetContentId(SettingsTitles.HIGHLOAD_API_USERS_CONTENT_ID);
            int servicesCoontentId = GetContentId(SettingsTitles.PRODUCT_SERVICES_CONTENT_ID);
            string field = method == "GET" ? "Read Service" : "Write Service";

            return string.Format(ServiceQueryTemplate, authorizationCoontentId, tokensCoontentId, servicesCoontentId, field);
        }

        private string GetCommonQuery(string method)
        {
            int authorizationCoontentId = GetContentId(SettingsTitles.API_AUTHORIZATION_CONTENT_ID);
            int tokensCoontentId = GetContentId(SettingsTitles.HIGHLOAD_API_USERS_CONTENT_ID);
            string field = method == "GET" ? "Read" : "Write";

            return string.Format(CommonQueryTemplate, authorizationCoontentId, tokensCoontentId, field);
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

            var sqlCommand = new SqlCommand();

            if (slug == null || version == null)
            {
                sqlCommand.CommandText = GetCommonQuery(method);
            }
            else
            {
                sqlCommand.CommandText = GetServiceQuery(method);
                sqlCommand.Parameters.AddWithValue("@slug", slug);
                sqlCommand.Parameters.AddWithValue("@version", version);
            }

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
    }
}