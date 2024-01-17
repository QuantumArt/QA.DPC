using System.Data.Common;
using Dapper;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.ContentProviders;
using System.Data.SqlClient;
using System.Globalization;
using Npgsql;
using QA.Core.DPC.QP.Models;
using QP.ConfigurationService.Models;
using Quantumart.QPublishing.Database;

namespace QA.Core.DPC.Loader.Editor
{
    public class EditorLocaleService
    {
        private readonly IUserProvider _userProvider;
        private readonly Customer _customer;

        public EditorLocaleService(
            IUserProvider userProvider,
            IConnectionProvider connectionProvider)
        {
            _userProvider = userProvider;
            _customer = connectionProvider.GetCustomer();
        }

        public CultureInfo GetCurrentUserCulture()
        {
            int userId = _userProvider.GetUserId();

			DbConnection connection = _customer.DatabaseType == DatabaseType.Postgres
				? (DbConnection)new NpgsqlConnection(_customer.ConnectionString)
				: new SqlConnection(_customer.ConnectionString); 
			using (connection)
			{
                connection.Open();

                int? culture = connection.QueryFirstOrDefault<int?>($@"
                    SELECT {SqlQuerySyntaxHelper.Top(_customer.DatabaseType, "1")} l.LOCALE
                    FROM {SqlQuerySyntaxHelper.DbSchemaName(_customer.DatabaseType)}LANGUAGES AS l
                    INNER JOIN {SqlQuerySyntaxHelper.DbSchemaName(_customer.DatabaseType)}USERS AS u ON l.LANGUAGE_ID = u.LANGUAGE_ID
                    WHERE u.USER_ID = @{nameof(userId)} {SqlQuerySyntaxHelper.Limit(_customer.DatabaseType, "1")}",
                    new { userId });

                return culture != null ? CultureInfo.GetCultureInfo(culture.Value) : null;
            }
        }
    }
}
