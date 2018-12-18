using Dapper;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.Infrastructure;
using System.Data.SqlClient;
using System.Globalization;

namespace QA.Core.DPC.Loader.Editor
{
    public class EditorLocaleService
    {
        private readonly IUserProvider _userProvider;
        private readonly string _connectionString;

        public EditorLocaleService(
            IUserProvider userProvider,
            IConnectionProvider connectionProvider)
        {
            _userProvider = userProvider;
            _connectionString = connectionProvider.GetConnection();
        }

        public CultureInfo GetCurrentUserCulture()
        {
            int userId = _userProvider.GetUserId();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                int? culture = connection.QueryFirstOrDefault<int?>($@"
                    SELECT TOP (1) l.LOCALE
                    FROM [dbo].[LANGUAGES] AS l
                    INNER JOIN [dbo].[USERS] AS u ON l.LANGUAGE_ID = u.LANGUAGE_ID
                    WHERE u.USER_ID = @{nameof(userId)}",
                    new { userId });

                return culture != null ? CultureInfo.GetCultureInfo(culture.Value) : null;
            }
        }
    }
}
