using System;
using System.Data;
using System.Linq;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QPublishing.Database;
using System.Data.SqlClient;
using Quantumart.QP8.Utils;
using System.Collections.Generic;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.ContentProviders;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL;

namespace QA.Core.DPC.Loader.Services
{
    public class FreezeService : IFreezeService
    {
        #region queries
        private const string UnfrosenProductsQuery =
        @"declare @query nvarchar(max) = ''

        declare @contentds Ids
        declare @contentQuery nvarchar(1000) = 'select distinct Content from content_' + cast(@definitionContentId as nvarchar(100)) + '_united where visible = 1 and archive = 0 and content <> @contentId'
        insert into @contentds exec sp_executesql @contentQuery, N'@contentId int', @contentId

        select
            @query = @query +
	        '
	        select
                [' + ex.ATTRIBUTE_NAME + '] Id
            from
                content_' + cast(ex.CONTENT_ID as nvarchar(100)) +'_united
            where
				visible = 1 and archive = 0 and	
                FreezeDate < @date  and
               [' + ex.ATTRIBUTE_NAME + '] is not null
            union'
        from
            content_attribute base
            join content_attribute ex on ex.CLASSIFIER_ATTRIBUTE_ID = base.ATTRIBUTE_ID
            join content_attribute f on ex.CONTENT_ID = f.CONTENT_ID
        where
	        base.content_id = @contentId and
	        base.ATTRIBUTE_NAME = @typeField and
            f.ATTRIBUTE_NAME = @freezeField

	    select
			@query = @query +
			'
			select
				CONTENT_ITEM_ID Id
			from
				content_' + cast(c.ID as nvarchar(100)) +'_united
			where
				visible = 1 and archive = 0 and
				FreezeDate < @date
			union'
		from
			@contentds c
			join content_attribute f on c.ID = f.CONTENT_ID
		where
			f.ATTRIBUTE_NAME = @freezeField
        
        if @query<> ''
        begin
         set @query = left(@query, len(@query) - len('union'))
         exec sp_executesql @query, N'@date datetime', @date
        end";

        private const string FrosenProductsQuery =
        @"declare @query nvarchar(max) = ''

        declare @contentds Ids
        declare @contentQuery nvarchar(1000) = 'select distinct Content from content_' + cast(@definitionContentId as nvarchar(100)) + '_united where visible = 1 and archive = 0 and content <> @contentId'
        insert into @contentds exec sp_executesql @contentQuery, N'@contentId int', @contentId

        select
	        @query = @query +
	        '
	        select
		        [' + ex.ATTRIBUTE_NAME + '] Id
	        from
		        content_' + cast(ex.CONTENT_ID as nvarchar(100)) +'_united
	        where
				visible = 1 and archive = 0 and
		        FreezeDate >= @date and
		        [' + ex.ATTRIBUTE_NAME + '] in (select Id from @ids)
	        union'
        from
	        content_attribute base
	        join content_attribute ex on ex.CLASSIFIER_ATTRIBUTE_ID = base.ATTRIBUTE_ID
	        join content_attribute f on ex.CONTENT_ID = f.CONTENT_ID
        where
	        base.content_id = @contentId and
	        base.ATTRIBUTE_NAME = @typeField and
	        f.ATTRIBUTE_NAME = @freezeField

        select
			@query = @query +
			'
			select
				CONTENT_ITEM_ID Id
			from
				content_' + cast(c.ID as nvarchar(100)) +'_united
			where
				visible = 1 and archive = 0 and	
				FreezeDate >= @date and
		        CONTENT_ITEM_ID in (select Id from @ids)
			union'
		from
			@contentds c
			join content_attribute f on c.ID = f.CONTENT_ID
		where
			f.ATTRIBUTE_NAME = @freezeField

        if @query <> ''
        begin
         set @query = left(@query, len(@query) - len('union'))
         exec sp_executesql @query, N'@date datetime, @ids Ids readonly', @date, @ids
        end";

        private const string FreezeStateQuery =
        @"declare @query nvarchar(max) = ''

        declare @contentds Ids
        declare @contentQuery nvarchar(1000) = 'select distinct Content from content_' + cast(@definitionContentId as nvarchar(100)) + '_united where visible = 1 and archive = 0 and content <> @contentId'
        insert into @contentds exec sp_executesql @contentQuery, N'@contentId int', @contentId

        select
	        @query = @query +
	        '
	        select
		        FreezeDate
	        from
		        content_' + cast(ex.CONTENT_ID as nvarchar(100)) +'_united
	        where
				visible = 1 and archive = 0 and		        
				[' + ex.ATTRIBUTE_NAME + '] = @id and
		        FreezeDate is not null
	        union'
        from
	        content_attribute base
	        join content_attribute ex on ex.CLASSIFIER_ATTRIBUTE_ID = base.ATTRIBUTE_ID
	        join content_attribute f on ex.CONTENT_ID = f.CONTENT_ID
        where
	        base.content_id = @contentId and
	        base.ATTRIBUTE_NAME = @typeField and
	        f.ATTRIBUTE_NAME = @freezeField

        select
			@query = @query +
			'
			select
				FreezeDate
			from
				content_' + cast(c.ID as nvarchar(100)) +'_united
			where
				visible = 1 and archive = 0 and		        
				CONTENT_ITEM_ID = @id and
				FreezeDate is not null
			union'
		from
			@contentds c
			join content_attribute f on c.ID = f.CONTENT_ID
		where
			f.ATTRIBUTE_NAME = @freezeField

        if @query <> ''
        begin
         set @query = left(@query, len(@query) - len('union'))
         exec sp_executesql @query, N'@id int', @id
        end";

        private const string ResetFreezingQuery =
        @"declare @query nvarchar(max) = ''

        declare @contentds Ids
        declare @contentQuery nvarchar(1000) = 'select distinct Content from content_' + cast(@definitionContentId as nvarchar(100)) + '_united where visible = 1 and archive = 0 and content <> @contentId'
        insert into @contentds exec sp_executesql @contentQuery, N'@contentId int', @contentId

        select
	        @query = @query +
	        '	
	        select
		        ' + cast(ex.CONTENT_ID as nvarchar(100)) + ' ContentId,
		        CONTENT_ITEM_ID Id,
		        ' + cast(f.ATTRIBUTE_ID as nvarchar(100)) + ' FieldId
	        from
		        content_' + cast(ex.CONTENT_ID as nvarchar(100)) +'_united
	        where
				visible = 1 and archive = 0 and		        
				[' + ex.ATTRIBUTE_NAME + '] in (select Id from @ids)
	        union'
        from
	        content_attribute base
	        join content_attribute ex on ex.CLASSIFIER_ATTRIBUTE_ID = base.ATTRIBUTE_ID
	        join content_attribute f on ex.CONTENT_ID = f.CONTENT_ID
        where
	        base.content_id = @contentId and
	        base.ATTRIBUTE_NAME = @typeField and
	        f.ATTRIBUTE_NAME = @freezeField

        select
	        @query = @query +
	        '
	        select
		        ' + cast(c.ID as nvarchar(100)) + ' ContentId,
		        CONTENT_ITEM_ID Id,
		        ' + cast(f.ATTRIBUTE_ID as nvarchar(100)) + ' FieldId
	        from
		        content_' + cast(c.ID as nvarchar(100)) +'_united
	        where
				visible = 1 and archive = 0 and		        
		        CONTENT_ITEM_ID in (select Id from @ids)
	        union'
        from
	        @contentds c
	        join content_attribute f on c.ID = f.CONTENT_ID
        where
	        f.ATTRIBUTE_NAME = @freezeField

        if @query <> ''
        begin
         set @query = left(@query, len(@query) - len('union'))
         exec sp_executesql @query, N'@ids Ids readonly', @ids
        end";
        #endregion

        private readonly ISettingsService _settingsService;
        private IUserProvider _userProvider;
        private string _connectionString;

        public FreezeService(ISettingsService settingsService, IUserProvider userProvider, IConnectionProvider connectionProvider)
        {
            _settingsService = settingsService;
            _userProvider = userProvider;
            _connectionString = connectionProvider.GetConnection();
        }

        #region ISettingsService implementation
        public FreezeState GetFreezeState(int productId)
        {
            int productContentId = GetProductContentId();
            int definitionContentId = GetDefinitionContentId();
            string typeField = GetTypeFieldName();
            string freezeField = GetFreezeFieldName();

            if (string.IsNullOrEmpty(freezeField))
            {
                return FreezeState.Missing;
            }

            var dbConnector = GetConnector();
            var sqlCommand = new SqlCommand(FreezeStateQuery);

            sqlCommand.Parameters.AddWithValue("@contentId", productContentId);
            sqlCommand.Parameters.AddWithValue("@definitionContentId", definitionContentId);
            sqlCommand.Parameters.AddWithValue("@typeField", typeField);
            sqlCommand.Parameters.AddWithValue("@freezeField", freezeField);
            sqlCommand.Parameters.AddWithValue("@id", productId);            

            var dt = dbConnector.GetRealData(sqlCommand);
            var dates = dt.AsEnumerable().Select(r => (DateTime)r["FreezeDate"]).ToArray();

            if (dates.Any())
            {
                var date = dates.First();
                if (date > DateTime.Now)
                {
                    return FreezeState.Frozen;
                }
                else
                {
                    return FreezeState.Unfrosen;
                }
            }
            else
            {
                return FreezeState.Missing;
            }            
        }

        public int[] GetFrosenProductIds(int[] productIds)
        {
            int productContentId = GetProductContentId();
            int definitionContentId = GetDefinitionContentId();
            string typeField = GetTypeFieldName();
            string freezeField = GetFreezeFieldName();

            if (string.IsNullOrEmpty(freezeField))
            {
                return new int[0];
            }

            var dbConnector = GetConnector();
            var sqlCommand = new SqlCommand(FrosenProductsQuery);

            sqlCommand.Parameters.AddWithValue("@contentId", productContentId);
            sqlCommand.Parameters.AddWithValue("@definitionContentId", definitionContentId);
            sqlCommand.Parameters.AddWithValue("@typeField", typeField);
            sqlCommand.Parameters.AddWithValue("@freezeField", freezeField);            
            sqlCommand.Parameters.AddWithValue("@date", DateTime.Now);
            sqlCommand.Parameters.Add(Common.GetIdsTvp(productIds, "@Ids"));

            var dt = dbConnector.GetRealData(sqlCommand);
            var ids = GetIds(dt);
            return ids;
        }

        public int[] GetUnfrosenProductIds()
        {
            int productContentId = GetProductContentId();
            int definitionContentId = GetDefinitionContentId();
            string typeField = GetTypeFieldName();
            string freezeField = GetFreezeFieldName();

            if (string.IsNullOrEmpty(freezeField))
            {
                return new int[0];
            }

            var dbConnector = GetConnector();

            var sqlCommand = new SqlCommand(UnfrosenProductsQuery);

            sqlCommand.Parameters.AddWithValue("@contentId", productContentId);
            sqlCommand.Parameters.AddWithValue("@definitionContentId", definitionContentId);
            sqlCommand.Parameters.AddWithValue("@typeField", typeField);
            sqlCommand.Parameters.AddWithValue("@freezeField", freezeField);
            sqlCommand.Parameters.AddWithValue("@date", DateTime.Now);

            var dt = dbConnector.GetRealData(sqlCommand);
            var ids = GetIds(dt);
            return ids;
        }

        public void ResetFreezing(params int[] productIds)
        {
            int userId = _userProvider.GetUserId();
            int productContentId = GetProductContentId();
            int definitionContentId = GetDefinitionContentId();
            string typeField = GetTypeFieldName();
            string freezeField = GetFreezeFieldName();

            if (string.IsNullOrEmpty(freezeField))
            {
                return;
            }

            var dbConnector = GetConnector();
            var sqlCommand = new SqlCommand(ResetFreezingQuery);

            sqlCommand.Parameters.AddWithValue("@contentId", productContentId);
            sqlCommand.Parameters.AddWithValue("@definitionContentId", definitionContentId);
            sqlCommand.Parameters.AddWithValue("@typeField", typeField);
            sqlCommand.Parameters.AddWithValue("@freezeField", freezeField);
            sqlCommand.Parameters.Add(Common.GetIdsTvp(productIds, "@Ids"));

            var dt = dbConnector.GetRealData(sqlCommand);

            var products = dt.AsEnumerable()
                .Select(r => Converter.ToModelFromDataRow<ProductDescriptor>(r))
                .ToArray();

            foreach(var g in products.GroupBy(p => new { p.ContentId, p.FieldId }))
            {
                var exstensionContentId = g.Key.ContentId;
                var fieldIds = new[] { g.Key.FieldId };

                var values = g.Select(p => new Dictionary<string, string>
                {
                    { FieldName.ContentItemId, p.Id.ToString() },
                    { freezeField, string.Empty }
                } ).ToArray();

                dbConnector.ImportToContent(exstensionContentId, values, userId, fieldIds);
            }
        }
        #endregion

        #region Private methods
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
        private string GetFreezeFieldName()
        {
            return _settingsService.GetSetting(SettingsTitles.PRODUCT_FREEZE_FIELD_NAME);
        }

        private string GetTypeFieldName()
        {
            return _settingsService.GetSetting(SettingsTitles.PRODUCT_TYPES_FIELD_NAME);
        }

        private int GetProductContentId()
        {
            return int.Parse(_settingsService.GetSetting(SettingsTitles.PRODUCTS_CONTENT_ID));
        }

        private int GetDefinitionContentId()
        {
            return int.Parse(_settingsService.GetSetting(SettingsTitles.PRODUCT_DEFINITIONS_CONTENT_ID));
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
