using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;
using Quantumart.QPublishing.Database;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace QA.Core.DPC.Loader.Services
{
    public class ValidationService : IValidationService
    {
        #region Queries
        private const string ProductQuery =
        @"
        declare @query nvarchar(max) = ''

        select
	        @query = @query +
	        '
	        select
		        ' + cast(a.CONTENT_ID as nvarchar(100)) + ' ContentId,
		        a.CONTENT_ITEM_ID Id,
		        a.[' + @publicationFailedField + '] PublicationFailed,
		        CONVERT(BIT, (CASE WHEN a.[' + @validationMessageField + '] IS NULL THEN 1 ELSE 0 END)) ValidationMessageIsEmpty
	        from
		        content_' + cast(a.CONTENT_ID as nvarchar(100)) +'_united a
		        join @ids ids on a.CONTENT_ITEM_ID = ids.ID
	        union'
        from content_item a
	        join @ids ids on a.CONTENT_ITEM_ID = ids.ID
	        join CONTENT_ATTRIBUTE publicationFailed on a.CONTENT_ID = publicationFailed.CONTENT_ID
	        join CONTENT_ATTRIBUTE validationMessage on a.CONTENT_ID = validationMessage.CONTENT_ID
        where
	        publicationFailed.ATTRIBUTE_NAME = @publicationFailedField and
	        validationMessage.ATTRIBUTE_NAME = @validationMessageField
        group by
	        a.CONTENT_ID
   
        if @query<> ''
        begin
            set @query = left(@query, len(@query) - len('union'))
            exec sp_executesql @query, N'@ids Ids readonly', @ids
        end";

        private const string AllProductsQuery = @"
        select
	        a.CONTENT_ITEM_ID Id
        from content_item a
	        join CONTENT_ATTRIBUTE publicationFailed on a.CONTENT_ID = publicationFailed.CONTENT_ID
	        join CONTENT_ATTRIBUTE validationMessage on a.CONTENT_ID = validationMessage.CONTENT_ID
        where
	        publicationFailed.ATTRIBUTE_NAME = @validationFailedField and
	        validationMessage.ATTRIBUTE_NAME = @validationMessageField and
	        a.ARCHIVE = 0";
        #endregion

        private const int UpdateChunkSize = 1000;
        private readonly ISettingsService _settingsService;
        private IUserProvider _userProvider;
        private ILogger _logger;
        private string _connectionString;

        public string PublicationFailedField => _settingsService.GetSetting(SettingsTitles.PRODUCT_PUBLICATION_FAILED_FIELD_NAME);
        public string ValidationFailedField => _settingsService.GetSetting(SettingsTitles.PRODUCT_VALIDATION_FAILED_FIELD_NAME);
        public string ValidationMessageField => _settingsService.GetSetting(SettingsTitles.PRODUCT_VALIDATION_MSG_FIELD_NAME);

        public ValidationService(ISettingsService settingsService, IUserProvider userProvider, IConnectionProvider connectionProvider, ILogger logger)
        {
            _settingsService = settingsService;
            _userProvider = userProvider;
            _logger = logger;
            _connectionString = connectionProvider.GetConnection();
        }

        #region IValidationService implementation
        public void UpdateValidationInfo(int[] productIds, ConcurrentDictionary<int, string> errors)
        {            
            if (!string.IsNullOrEmpty(PublicationFailedField) && !string.IsNullOrEmpty(ValidationMessageField))
            {
                var dbConnector = GetConnector();
                int userId = _userProvider.GetUserId();
                var products = GetProducts(dbConnector, productIds)
                    .Where(p => errors.ContainsKey(p.Id) || p.PublicationFailed || !p.ValidationMessageIsEmpty)
                    .ToArray();

                foreach (var g in products.GroupBy(p =>  p.ContentId ))
                {
                    var articles = new List<Dictionary<string, string>>();

                    foreach (var product in g)
                    {
                        var failed = errors.TryGetValue(product.Id, out string error);

                        var values = new Dictionary<string, string>
                        {
                            { FieldName.ContentItemId, product.Id.ToString(CultureInfo.InvariantCulture) },
                            { PublicationFailedField, failed ? "1" : "0" },
                            { ValidationMessageField, failed ? error : string.Empty }
                        };

                        articles.Add(values);
                    }

                    foreach (var chunk in articles.Section(UpdateChunkSize))
                    {
                        dbConnector.MassUpdate(g.Key, chunk, userId);
                    }

                    _logger.LogInfo(() => $"Update validation statuses for content {g.Key} in articles: {string.Join(", ", productIds)}, validation errors in: {string.Join(", ", errors.Keys)}");
                }
            }
        }
        public void ValidateAndUpdate(int[] productIds, Dictionary<int, string> errors)
        {
            var dbConnector = GetConnector();
            int userId = _userProvider.GetUserId();
            var products = GetProducts(dbConnector, productIds);

            errors.Add(productIds[0], "error");
        }

        public int[] GetProductIds()
        {
            if (!string.IsNullOrEmpty(ValidationFailedField) && !string.IsNullOrEmpty(ValidationMessageField))
            {
                var sqlCommand = new SqlCommand(ProductQuery);

                sqlCommand.Parameters.AddWithValue("@validationFailedField", ValidationFailedField);
                sqlCommand.Parameters.AddWithValue("@validationMessageField", ValidationMessageField);

                var dt = GetConnector().GetRealData(sqlCommand);

                var products = dt.AsEnumerable()
                    .Select(r => (int)(decimal)r["Id"])
                    .ToArray();

                return products;
            }
            else
            {
                return new int[0];
            }

        }
        #endregion

        #region Private methods
        private ProductDescriptor[] GetProducts(DBConnector dbConnector, int[] productIds)
        {            
            var sqlCommand = new SqlCommand(ProductQuery);

            sqlCommand.Parameters.AddWithValue("@publicationFailedField", PublicationFailedField);
            sqlCommand.Parameters.AddWithValue("@validationMessageField", ValidationMessageField);
            sqlCommand.Parameters.Add(Common.GetIdsTvp(productIds, "@Ids"));

            var dt = dbConnector.GetRealData(sqlCommand);

            var products = dt.AsEnumerable()
                .Select(r => Converter.ToModelFromDataRow<ProductDescriptor>(r))
                .ToArray();

            return products;
        }

        private DBConnector GetConnector()
        {
            var scope = QPConnectionScope.Current;

            if (scope != null && scope.DbConnection != null)
            {
                return new DBConnector(scope.DbConnection);
            }
            else
            {
                return new DBConnector(_connectionString);
            }
        }
        #endregion

        private class ProductDescriptor
        {
            public int ContentId { get; set; }
            public int Id { get; set; }
            public bool PublicationFailed { get; set; }
            public bool ValidationMessageIsEmpty { get; set; }
        }
    }
}
