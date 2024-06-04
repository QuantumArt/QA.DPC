using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL;
using Quantumart.QPublishing.Database;
using System.Data.SqlClient;
using QA.ProductCatalog.ContentProviders;
using QA.Core.DPC.QP.Models;

namespace QA.Core.DPC.Loader.Services
{
    public class ProductContentResolver : IProductContentResolver
    {
        private readonly ISettingsService _settingsService;
        private readonly Customer _customer;

        private const string QueryTemplate =
            @"select	
                c.CONTENT_ID CONTENT_ITEM_ID
            from
                CONTENT_{0}_UNITED d
                JOIN CONTENT c ON d.Content = c.CONTENT_ID
                JOIN CONTENT_ATTRIBUTE a ON a.CONTENT_ID = c.CONTENT_ID
            where
                a.ATTRIBUTE_NAME = 'Type' and
                a.ATTRIBUTE_TYPE_ID = 1 and
                a.DEFAULT_VALUE = @type";

        public ProductContentResolver(ISettingsService settingsService, IConnectionProvider connectionProvider)
        {
            _settingsService = settingsService;
            _customer = connectionProvider.GetCustomer();
        }

        public int GetContentIdByType(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                return GetProductContentId();
            }
            else
            {
                var productDefinitionsContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.PRODUCT_DEFINITIONS_CONTENT_ID));

                var connector = _customer.DbConnector;
                var dbCommand = connector.CreateDbCommand(string.Format(QueryTemplate, productDefinitionsContentId));
                dbCommand.Parameters.AddWithValue("@type", type);
                var contentId = connector.GetRealScalarData(dbCommand);

                if (contentId == null)
                {
                    return GetProductContentId();
                }
                else
                {
                    return (int)(decimal)contentId;
                }
            }
        }

        private int GetProductContentId()
        {
            return int.Parse(_settingsService.GetSetting(SettingsTitles.PRODUCTS_CONTENT_ID));
        }

        private DBConnector GetConnector()
        {
            var scope = QPConnectionScope.Current;

            if (scope != null && scope.DbConnection != null)
            {
                return new DBConnector(scope.DbConnection);
            }
            return _customer.DbConnector;
        }
    }
}
