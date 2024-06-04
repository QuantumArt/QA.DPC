using System;
using System.Data;
using System.Linq;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.Utils;
using System.Collections.Generic;
using System.Runtime.Caching;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.ContentProviders;
using Quantumart.QP8.BLL;
using QA.Core.DPC.QP.Models;
using QA.Core.Logger;
using Quantumart.QP8.Constants;
using Quantumart.QPublishing.Database;

namespace QA.Core.DPC.Loader.Services
{
    public class FreezeService : IFreezeService
    {
	    
	    private readonly ISettingsService _settingsService;
	    private readonly IUserProvider _userProvider;
	    private readonly Customer _customer;
	    private readonly string _freezeFieldName;
	    private readonly string _typeFieldName;
	    private readonly int _productContentId;
	    private readonly int _definitionContentId;
	    private readonly MemoryCache _cache = MemoryCache.Default;
	    private readonly ILogger _logger;
	    private const string ExtentionFreezeMetaCacheKey = "extension_freeze_meta";
	    private const string ProductFreezeMetaCacheKey = "product_freeze_meta";
	    private const int FreezeMetaCacheDuration = 5;
        
        public FreezeService(ISettingsService settingsService, IUserProvider userProvider, IConnectionProvider connectionProvider, ILogger logger)
        {
            _settingsService = settingsService;
            _userProvider = userProvider;
            _customer = connectionProvider.GetCustomer();
            _freezeFieldName = _settingsService.GetSetting(SettingsTitles.PRODUCT_FREEZE_FIELD_NAME);
            _typeFieldName = _settingsService.GetSetting(SettingsTitles.PRODUCT_TYPES_FIELD_NAME);
            _productContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.PRODUCTS_CONTENT_ID));
            _definitionContentId =
	            int.Parse(_settingsService.GetSetting(SettingsTitles.PRODUCT_DEFINITIONS_CONTENT_ID));
            _logger = logger;
        }
        
        private class ExtentionFreezeMetaRow
        {
	        public int ContentId { get; set; }
	        public int AttributeId { get; set; }
	        public string AttributeName { get; set; }
        }

        private class ProductFreezeMetaRow
        {
	        public int ContentId { get; set; }
	        public int AttributeId { get; set; }
        }

        #region ISettingsService implementation

        private IEnumerable<ExtentionFreezeMetaRow> GetExtensionsFreezeMeta()
        {
	        if (_cache.Get(ExtentionFreezeMetaCacheKey) is ExtentionFreezeMetaRow[] meta)
	        {
		        return meta;
	        }
	        _logger.Debug($"'{ExtentionFreezeMetaCacheKey}' cache miss");

	        var query = @"
					select ex.CONTENT_ID, f.ATTRIBUTE_ID, ex.ATTRIBUTE_NAME 
					from content_attribute base
						join content_attribute ex on ex.CLASSIFIER_ATTRIBUTE_ID = base.ATTRIBUTE_ID
						join content_attribute f on ex.CONTENT_ID = f.CONTENT_ID
					where
						base.content_id = @contentId and
						base.ATTRIBUTE_NAME = @typeField and
						f.ATTRIBUTE_NAME = @freezeField";
	        
	        var dbConnector = GetConnector();
	        var dbCommand = dbConnector.CreateDbCommand(query);
	        dbCommand.Parameters.AddWithValue("@contentId", _productContentId);
	        dbCommand.Parameters.AddWithValue("@typeField", _typeFieldName);
	        dbCommand.Parameters.AddWithValue("@freezeField", _freezeFieldName);
	        var metaData = dbConnector.GetRealData(dbCommand);
	        List<ExtentionFreezeMetaRow> metaRows = new List<ExtentionFreezeMetaRow>(metaData.Rows.Count);
	        foreach (DataRow row in metaData.Rows)
	        {
		        metaRows.Add(new ExtentionFreezeMetaRow
		        {
			        ContentId = int.Parse(row["CONTENT_ID"].ToString()),
			        AttributeId = int.Parse(row["ATTRIBUTE_ID"].ToString()),
			        AttributeName = row["ATTRIBUTE_NAME"].ToString()
		        });
	        }
	        _cache.Add(ExtentionFreezeMetaCacheKey, metaRows, DateTimeOffset.UtcNow.AddMinutes(FreezeMetaCacheDuration));
	        return metaRows;
        }

        private IEnumerable<ProductFreezeMetaRow> GetProductFreezeMeta()
        {
	        
	        if (_cache.Get(ProductFreezeMetaCacheKey) is ProductFreezeMetaRow[] meta)
	        {
		        return meta;
	        }
	        _logger.Debug($"'{ProductFreezeMetaCacheKey}' cache miss");
	        
	        var query = $@"SELECT c.CONTENT_ID, f.ATTRIBUTE_ID FROM 
				(SELECT DISTINCT Content AS CONTENT_ID FROM content_{_definitionContentId}_united 
					WHERE visible = 1 AND archive = 0) AS c
				JOIN CONTENT_ATTRIBUTE f ON c.CONTENT_ID = f.CONTENT_ID
				WHERE f.ATTRIBUTE_NAME = @freezeField";
	        var dbConnector = GetConnector();
	        var dbCommand = dbConnector.CreateDbCommand(query);
	        dbCommand.Parameters.AddWithValue("@freezeField", _freezeFieldName);
	        var metaData = dbConnector.GetRealData(dbCommand);
	        List<ProductFreezeMetaRow> metaRows = new List<ProductFreezeMetaRow>(metaData.Rows.Count);
	        foreach (DataRow row in metaData.Rows)
	        {
		        metaRows.Add(new ProductFreezeMetaRow
		        {
			        ContentId = int.Parse(row["CONTENT_ID"].ToString()),
			        AttributeId = int.Parse(row["ATTRIBUTE_ID"].ToString())
		        });
	        }
	        _cache.Add(ProductFreezeMetaCacheKey, metaRows, DateTimeOffset.UtcNow.AddMinutes(FreezeMetaCacheDuration));
	        return metaRows;
        }
        
        public FreezeState GetFreezeState(int productId)
        {
	        if (string.IsNullOrEmpty(_freezeFieldName))
	        {
		        return FreezeState.Missing;
	        }

            List<string> freezeFieldQueries = new List<string>();
            var extensionFreezeMeta = GetExtensionsFreezeMeta();
            foreach (ExtentionFreezeMetaRow row in extensionFreezeMeta)
            {
	            freezeFieldQueries.Add($@"select {_freezeFieldName} from content_{row.ContentId}_united
						where visible = 1 and archive = 0 and {row.AttributeName} = @id
								and {_freezeFieldName} is not null");
            }
            

            var productFreezeMeta = GetProductFreezeMeta();
            
            foreach (ProductFreezeMetaRow row in productFreezeMeta)
            {
	            freezeFieldQueries.Add($@"select {_freezeFieldName} 
						from content_{row.ContentId}_united
						where visible = 1 and archive = 0 
							and CONTENT_ITEM_ID = @id
							and {_freezeFieldName} is not null");
            }

            if (!freezeFieldQueries.Any()) return FreezeState.Missing;
            
            var dbConnector = GetConnector();
            var dbCommand = dbConnector.CreateDbCommand(string.Join("\nunion all\n", freezeFieldQueries));
            dbCommand.Parameters.AddWithValue("@id", productId);            
            
            var freezeDatesData = dbConnector.GetRealData(dbCommand);
            var dates = freezeDatesData.AsEnumerable().Select(r => (DateTime)r[_freezeFieldName]).ToArray();

            if (!dates.Any()) return FreezeState.Missing;
            return dates.First() > DateTime.Now ? FreezeState.Frozen : FreezeState.Unfrosen;
        }

        public int[] GetFrozenProductIds(int[] productIds)
        {
            if (string.IsNullOrEmpty(_freezeFieldName))
            {
                return new int[0];
            }

            List<string> freezeFieldQueries = new List<string>();
            var extensionFreezeMeta = GetExtensionsFreezeMeta();
            var dbConnector = GetConnector();

            string idsQueryPart = SqlQuerySyntaxHelper.IdList(dbConnector.DatabaseType, "@Ids", "Ids");
            
            foreach (ExtentionFreezeMetaRow row in extensionFreezeMeta)
            {
	            freezeFieldQueries.Add($@"SELECT {row.AttributeName} Id FROM content_{row.ContentId}_united
						WHERE visible = 1 AND archive = 0
								AND {_freezeFieldName} >= @date 
								AND {row.AttributeName} in (
									SELECT Id 
									FROM {idsQueryPart})");
            }
            
            var productFreezeMeta = GetProductFreezeMeta();
            
            foreach (ProductFreezeMetaRow row in productFreezeMeta)
            {
	            freezeFieldQueries.Add($@"SELECT CONTENT_ITEM_ID Id FROM content_{row.ContentId}_united
						WHERE visible = 1 AND archive = 0 
								AND {_freezeFieldName} >= @date 
								AND CONTENT_ITEM_ID in (
									SELECT Id 
									FROM {idsQueryPart})");
            }
            
            var dbCommand = dbConnector.CreateDbCommand(string.Join("\nunion all\n", freezeFieldQueries));
            dbCommand.Parameters.AddWithValue("@date", DateTime.Now);
            dbCommand.Parameters.Add(SqlQuerySyntaxHelper.GetIdsDatatableParam("@Ids", productIds, dbConnector.DatabaseType));

            var freezeDatesData = dbConnector.GetRealData(dbCommand);
            var ids = GetIds(freezeDatesData);
            return ids;
        }

        public int[] GetUnfrozenProductIds()
        {
	        if (string.IsNullOrEmpty(_freezeFieldName))
	        {
		        return new int[0];
	        }

	        List<string> freezeFieldQueries = new List<string>();
	        var extensionFreezeMeta = GetExtensionsFreezeMeta();
	        var dbConnector = GetConnector();

	        foreach (ExtentionFreezeMetaRow row in extensionFreezeMeta)
	        {
		        freezeFieldQueries.Add($@"SELECT {row.AttributeName} Id FROM content_{row.ContentId}_united
						WHERE visible = 1 AND archive = 0
								AND {_freezeFieldName} < @date 
								AND {row.AttributeName} IS NOT NULL");
	        }
            
	        var productFreezeMeta = GetProductFreezeMeta();
            
	        foreach (ProductFreezeMetaRow row in productFreezeMeta)
	        {
		        freezeFieldQueries.Add($@"SELECT CONTENT_ITEM_ID Id FROM content_{row.ContentId}_united
						WHERE visible = 1 AND archive = 0 
								AND {_freezeFieldName} < @date");
	        }
            
	        var dbCommand = dbConnector.CreateDbCommand(string.Join("\nunion all\n", freezeFieldQueries));
	        dbCommand.Parameters.AddWithValue("@date", DateTime.Now);

	        var freezeDatesData = dbConnector.GetRealData(dbCommand);
	        var ids = GetIds(freezeDatesData);
	        return ids;
        }

        public void ResetFreezing(int[] productIds)
        {
            if (string.IsNullOrEmpty(_freezeFieldName))
            {
                return;
            }
            int userId = _userProvider.GetUserId();

            List<string> freezeFieldQueries = new List<string>();
            var extensionFreezeMeta = GetExtensionsFreezeMeta();
            var dbConnector = GetConnector();


            string idsQueryPart = SqlQuerySyntaxHelper.IdList(dbConnector.DatabaseType, "@Ids", "Ids");
            
            foreach (ExtentionFreezeMetaRow row in extensionFreezeMeta)
            {
	            freezeFieldQueries.Add($@"SELECT {row.ContentId} AS ContentId, CONTENT_ITEM_ID AS Id, {row.AttributeId} AS FieldId  
						FROM content_{row.ContentId}_united
						WHERE visible = 1 AND archive = 0
								AND {row.AttributeName} in (
									SELECT Id 
									FROM {idsQueryPart})");
            }

            var productFreezeMeta = GetProductFreezeMeta();
            
            foreach (ProductFreezeMetaRow row in productFreezeMeta)
            {
	            freezeFieldQueries.Add($@"SELECT {row.ContentId} AS ContentId, CONTENT_ITEM_ID AS Id, {row.AttributeId} AS FieldId 
						FROM content_{row.ContentId}_united
						WHERE visible = 1 AND archive = 0 
								AND CONTENT_ITEM_ID in (
									SELECT Id 
									FROM {idsQueryPart})");
            }
            var dbCommand = dbConnector.CreateDbCommand(string.Join("\nunion all\n", freezeFieldQueries));
            dbCommand.Parameters.Add(SqlQuerySyntaxHelper.GetIdsDatatableParam("@Ids", productIds, dbConnector.DatabaseType));

            var freezeDatesData = dbConnector.GetRealData(dbCommand);
            var products = freezeDatesData.AsEnumerable()
                .Select(Converter.ToModelFromDataRow<ProductDescriptor>)
                .ToArray();

            foreach(var g in products.GroupBy(p => new { p.ContentId, p.FieldId }))
            {
                var extentionContentId = g.Key.ContentId;
                var fieldIds = new[] { g.Key.FieldId };

                var values = g.Select(p => new Dictionary<string, string>
                {
                    { FieldName.ContentItemId, p.Id.ToString() },
                    { _freezeFieldName, string.Empty }
                } ).ToArray();
				
	            dbConnector.ImportToContent(extentionContentId, values, userId, fieldIds);
            }
        }
        #endregion

        #region Private methods
        private DBConnector GetConnector()
        {
            var scope = QPConnectionScope.Current;

            if (scope?.DbConnection != null)
            {
                return new DBConnector(scope.DbConnection){WithTransaction = false};
            }

            return _customer.DbConnector;
        }


        private int[] GetIds(DataTable dt)
        {
            return dt.AsEnumerable()
                .Select(r => (int)(decimal)r["Id"])
                .ToArray();
        }
        #endregion

        private class ProductDescriptor
        {
            public int ContentId { get; set; }
            public int Id { get; set; }
            public int FieldId { get; set; }
        }
    }
}
