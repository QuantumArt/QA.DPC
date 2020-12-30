using System;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.ContentProviders;
using QP.ConfigurationService.Models;
using Quantumart.QPublishing.Database;

namespace QA.ProductCatalog.WebApi.Filters
{
    public class IdentityResolverAttribute : TypeFilterAttribute
    {
        public IdentityResolverAttribute() : base(typeof(IdentityResolverImpl))
        {
        }

        private class IdentityResolverImpl : Attribute, IActionFilter
        {
            private const int DefaultUserId = 1;

            private string CommonQueryTemplate = @"
            select auth.{3} AS UserId
            from
	            content_{0}_united AS auth
	            join content_{1}_united tokens on auth.{4} = tokens.CONTENT_ITEM_ID
            where
	            (@token is null and tokens.Name = 'Default' or tokens.AccessToken = @token) and
	            auth.{2} = 1 and
                auth.Visible = 1 and
	            auth.Archive = 0 and
	            tokens.Visible = 1 and
	            tokens.Archive = 0";

            private string ServiceQueryTemplate = @"select
	            auth.{3} AS UserId
            from content_{0}_united AS auth
            join content_{1}_united AS tokens on auth.{4} = tokens.CONTENT_ITEM_ID
            join item_to_item AS link on auth.CONTENT_ITEM_ID = link.l_item_id and auth.{5} = link.link_id
            join content_{2}_united services on link.r_item_id = services.CONTENT_ITEM_ID
            where
            (@token is null and tokens.Name = 'Default' or tokens.AccessToken = @token) 
            and lower(services.Slug) = @slug and lower(services.Version) = @version 
            and auth.Visible = 1 and auth.Archive = 0 
            and tokens.Visible = 1 and tokens.Archive = 0 
            and services.Visible = 1 and services.Archive = 0";


            private readonly IIdentityProvider _identityProvider;
            private readonly IConnectionProvider _connectionProvider;
            private readonly ISettingsService _settingsService;
            private readonly Properties _props;

            public IdentityResolverImpl(
                IIdentityProvider identityProvider,
                IConnectionProvider connectionProvider,
                ISettingsService settingsService,
                IOptions<Properties> props
            )
            {
                _identityProvider = identityProvider;
                _connectionProvider = connectionProvider;
                _settingsService = settingsService;
                _props = props.Value;
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                var customerCode = context.RouteData.Values["customerCode"]?.ToString() ?? SingleCustomerCoreProvider.Key;
                Identity identity = null;

                if (UseAuthorization())
                {
                    var method = context.HttpContext.Request.Method;
                    var token = context.HttpContext.Request.Headers["X-Auth-Token"];
                    var slug = context.RouteData.Values["slug"].ToString();
                    var version = context.RouteData.Values["version"].ToString();
                    var userId = GetUserId(slug, version, token, method);

                    if (userId.HasValue)
                    {
                        var userName = GetUserName(userId.Value);
                        identity = new Identity(customerCode, userId.Value, userName, true);
                    }
                    else
                    {
                        identity = new Identity(customerCode);
                        context.Result = new UnauthorizedResult();
                    }
                }
                else
                {
                    var defaultUserId = GetDefaultUserId();
                    var defaultUserName = GetUserName(defaultUserId);
                    identity = new Identity(customerCode, defaultUserId, defaultUserName, true);
                }

                _identityProvider.Identity = identity;
            }

            private string GetServiceQuery(string method, DatabaseType dbType)
            {
                int authorizationContentId = GetContentId(SettingsTitles.API_AUTHORIZATION_CONTENT_ID);
                int tokensContentId = GetContentId(SettingsTitles.HIGHLOAD_API_USERS_CONTENT_ID);
                int servicesContentId = GetContentId(SettingsTitles.PRODUCT_SERVICES_CONTENT_ID);
                var qp = SqlQuerySyntaxHelper.EscapeEntityName(dbType, "Qp User");
                var token = SqlQuerySyntaxHelper.EscapeEntityName(dbType, "Token User");
                var fieldName = method == "GET" ? "Read Service" : "Write Service";
                var field = SqlQuerySyntaxHelper.EscapeEntityName(dbType, fieldName);
                
                return string.Format(ServiceQueryTemplate, authorizationContentId, tokensContentId,
                    servicesContentId, qp, token, field);
            }

            private string GetCommonQuery(string method, DatabaseType dbType)
            {
                int authorizationCoontentId = GetContentId(SettingsTitles.API_AUTHORIZATION_CONTENT_ID);
                int tokensCoontentId = GetContentId(SettingsTitles.HIGHLOAD_API_USERS_CONTENT_ID);
                var fieldName = method == "GET" ? "Read" : "Write";
                var field = SqlQuerySyntaxHelper.EscapeEntityName(dbType, fieldName);                
                var qp = SqlQuerySyntaxHelper.EscapeEntityName(dbType, "Qp User");
                var token = SqlQuerySyntaxHelper.EscapeEntityName(dbType, "Token User");
                return string.Format(CommonQueryTemplate, authorizationCoontentId, tokensCoontentId, field, qp, token);
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

            private int GetDefaultUserId()
            {
                if (_props != null && _props.UserId != 0)
                    return _props.UserId;

                return DefaultUserId;
            }

            private int? GetUserId(string slug, string version, string token, string method)
            {
                var customer = _connectionProvider.GetCustomer();
                var dbConnector = new DBConnector(customer.ConnectionString, customer.DatabaseType);

                var dbCommand = dbConnector.CreateDbCommand();

                if (slug == null || version == null)
                {
                    dbCommand.CommandText = GetCommonQuery(method, customer.DatabaseType);
                }
                else
                {
                    dbCommand.CommandText = GetServiceQuery(method, customer.DatabaseType);
                    dbCommand.Parameters.AddWithValue("@slug", slug.ToLower());
                    dbCommand.Parameters.AddWithValue("@version", version.ToLower());
                }

                var tokenParameter = dbCommand.Parameters.AddWithValue("@token", (object) token ?? DBNull.Value);
                tokenParameter.DbType = DbType.String;

                var dt = dbConnector.GetRealData(dbCommand);
                var userIds = dt.AsEnumerable()
                    .Select(row => row["UserId"] == DBNull.Value ? null : (int?) (decimal?) row["UserId"]).ToArray();

                if (userIds.Any())
                {
                    return userIds.First() ?? GetDefaultUserId();
                }
                return null;
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
            }

            private string GetUserName(int userId)
            {
                var customer = _connectionProvider.GetCustomer();
                var dbConnector = new DBConnector(customer.ConnectionString, customer.DatabaseType);

                var dbCommand = dbConnector.CreateDbCommand();

                dbCommand.CommandText = $@"select first_name {(customer.DatabaseType == DatabaseType.Postgres ? "|| ' ' ||" : "+ ' ' +")} last_name as name 
                    from users where user_id = @id";
                dbCommand.Parameters.AddWithValue("@id", userId);

                var dt = dbConnector.GetRealData(dbCommand);
                return dt.Rows.Count > 0 ? (string) dt.Rows[0]["name"] : String.Empty;
            }

            private bool UseAuthorization()
            {
                return _props?.UseAuthorization ?? false;
            }
        }
    }
}