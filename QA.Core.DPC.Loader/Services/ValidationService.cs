using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;
using Quantumart.QPublishing.Database;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

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
		        a.[' + @validationFailedField + '] PublicationFailed,
		        CONVERT(BIT, (CASE WHEN a.[' + @validationMessageField + '] IS NULL THEN 1 ELSE 0 END)) ValidationMessageIsEmpty
	        from
		        content_' + cast(a.CONTENT_ID as nvarchar(100)) +'_united a
		        join @ids ids on a.CONTENT_ITEM_ID = ids.ID
            where
	            a.ARCHIVE = 0 and
	            a.VISIBLE = 1
	        union all'
        from content_item a
	        join @ids ids on a.CONTENT_ITEM_ID = ids.ID
	        join CONTENT_ATTRIBUTE publicationFailed on a.CONTENT_ID = publicationFailed.CONTENT_ID
	        join CONTENT_ATTRIBUTE validationMessage on a.CONTENT_ID = validationMessage.CONTENT_ID
        where
	        publicationFailed.ATTRIBUTE_NAME = @validationFailedField and
	        validationMessage.ATTRIBUTE_NAME = @validationMessageField and
	        a.ARCHIVE = 0 and
	        a.VISIBLE = 1
        group by
	        a.CONTENT_ID
   
        if @query<> ''
        begin
            set @query = left(@query, len(@query) - len('union all'))
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
	        a.ARCHIVE = 0 and
            a.VISIBLE = 1";
        #endregion

        private const int UpdateChunkSize = 1000;
        private readonly ISettingsService _settingsService;
        private readonly IUserProvider _userProvider;
        private readonly IArticleService _articleService;
        private readonly ILogger _logger;
        private readonly string _connectionString;

        public string PublicationFailedField => _settingsService.GetSetting(SettingsTitles.PRODUCT_PUBLICATION_FAILED_FIELD_NAME);
        public string ValidationFailedField => _settingsService.GetSetting(SettingsTitles.PRODUCT_VALIDATION_FAILED_FIELD_NAME);
        public string ValidationMessageField => _settingsService.GetSetting(SettingsTitles.PRODUCT_VALIDATION_MSG_FIELD_NAME);

        public ValidationService(ISettingsService settingsService, IUserProvider userProvider, IConnectionProvider connectionProvider, IArticleService articleService, ILogger logger)
        {
            _settingsService = settingsService;
            _userProvider = userProvider;
            _articleService = articleService;
            _logger = logger;
            _connectionString = connectionProvider.GetConnection();
        }

        #region IValidationService implementation
        public void UpdateValidationInfo(int[] productIds, ConcurrentDictionary<int, string> errors)
        {            
            if (!string.IsNullOrEmpty(PublicationFailedField) && !string.IsNullOrEmpty(ValidationMessageField))
            {
                int userId = _userProvider.GetUserId();
                UpdateValidationInfo(productIds, errors, PublicationFailedField, ValidationMessageField, userId);
            }
        }
        public ValidationReport ValidateAndUpdate(int chunkSize, int maxDegreeOfParallelism, ITaskExecutionContext context)
        {
            Console.WriteLine(new{ maxDegreeOfParallelism, chunkSize });
            var report = new ValidationReport();

            if (!string.IsNullOrEmpty(ValidationFailedField) && !string.IsNullOrEmpty(ValidationMessageField))
            {
                var productIds = GetProductIds();
                int userId = _userProvider.GetUserId();

                if (userId == 0)
                {
                    throw new Exception("userId is not defined");
                }

                Parallel.ForEach(productIds.Section(chunkSize), new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, (chunk, state) =>
                {
                    var errors = new ConcurrentDictionary<int, string>();
                    int n = 0;

                    foreach (var productId in chunk)
                    {
                        if (state.IsStopped)
                        {
                            return;
                        }
                        else if (n % maxDegreeOfParallelism == 0 && context.IsCancellationRequested)
                        {
                            context.IsCancelled = true;
                            state.Stop();
                            return;
                        }

                        n++;

                        try
                        {                            
                            var validation = _articleService.XamlValidationById(productId, true);

                            if (!validation.IsEmpty)
                            {
                                var value = string.Join(Environment.NewLine, validation.Errors.Select(s => s.Message));
                                errors.TryAdd(productId, value);
                                report.InvalidProductsCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            errors.TryAdd(productId, ex.Message);
                            report.ValidationErrorsCount++;
                            _logger.ErrorException($"Error while validating product {productId}", ex);
                        }

                        byte progress = (byte)(++report.TotalProductsCount * 100 / productIds.Length);
                        context.SetProgress(progress);
                    }

                    var updateResult = UpdateValidationInfo(chunk.ToArray(), errors, ValidationFailedField, ValidationMessageField, userId);
                    report.ValidatedProductsCount += updateResult.ProductsCount;
                    report.UpdatedProductsCount += updateResult.UpdatedProuctsCount;
                });
            }

            return report;
        }

        private int[] GetProductIds()
        {
            if (!string.IsNullOrEmpty(ValidationFailedField) && !string.IsNullOrEmpty(ValidationMessageField))
            {
                var sqlCommand = new SqlCommand(AllProductsQuery);

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
        private ProductDescriptor[] GetProducts(DBConnector dbConnector, int[] productIds, string validationFailedField, string validationMessageField)
        {            
            var sqlCommand = new SqlCommand(ProductQuery);

            sqlCommand.Parameters.AddWithValue("@validationFailedField", validationFailedField);
            sqlCommand.Parameters.AddWithValue("@validationMessageField", validationMessageField);
            sqlCommand.Parameters.Add(Common.GetIdsTvp(productIds, "@Ids"));

            var dt = dbConnector.GetRealData(sqlCommand);

            var products = dt.AsEnumerable()
                .Select(r => Converter.ToModelFromDataRow<ProductDescriptor>(r))
                .ToArray();

            return products;
        }

        private ValidationInfo UpdateValidationInfo(int[] productIds, ConcurrentDictionary<int, string> errors, string validationFailedField, string validationMessageField, int userId)
        {
            var result = new ValidationInfo();
            var dbConnector = GetConnector();            

            var products = GetProducts(dbConnector, productIds, validationFailedField, validationMessageField);
            result.ProductsCount = products.Length;

            var productsToUpdate = products
                .Where(p => errors.ContainsKey(p.Id) || p.PublicationFailed || !p.ValidationMessageIsEmpty)
                .ToArray();
            result.UpdatedProuctsCount = productsToUpdate.Length;

            foreach (var g in productsToUpdate.GroupBy(p => p.ContentId))
            {
                var articles = new List<Dictionary<string, string>>();

                foreach (var product in g)
                {
                    var failed = errors.TryGetValue(product.Id, out string error);

                    var values = new Dictionary<string, string>
                        {
                            { FieldName.ContentItemId, product.Id.ToString(CultureInfo.InvariantCulture) },
                            { validationFailedField, failed ? "1" : "0" },
                            { validationMessageField, failed ? error : string.Empty }
                        };

                    articles.Add(values);
                }

                foreach (var chunk in articles.Section(UpdateChunkSize))
                {
                    dbConnector.MassUpdate(g.Key, chunk, userId);
                }

                _logger.LogInfo(() => $"Update validation statuses for content {g.Key} in articles: {string.Join(", ", productIds)}, validation errors in: {string.Join(", ", errors.Keys)}");
            }

            return result;
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

        private class ValidationInfo
        {
            public int ProductsCount { get; set; }
            public int UpdatedProuctsCount { get; set; }
        }
    }
}
