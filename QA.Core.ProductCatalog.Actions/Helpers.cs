﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.QP.Models;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QPublishing.Database;

namespace QA.Core.ProductCatalog.Actions
{
    class Helpers
    {
        public static int[] GetProductIdsFromMarketingProducts(int[] marketingProductIds, IArticleService articleService, ISettingsService settingsService)
        {
            int marketingProductContentId = int.Parse(settingsService.GetSetting(SettingsTitles.MARKETING_PRODUCT_CONTENT_ID));

            var marketingProducts = articleService.List(marketingProductContentId, marketingProductIds).ToArray();

            string productsFieldName = settingsService.GetSetting(SettingsTitles.MARKETING_PRODUCT_PRODUCTS_FIELD_NAME);

            return
                (from product in marketingProducts
                    from fv in product.FieldValues
                    where fv.Field.Name == productsFieldName
                        from id in fv.RelatedItems
                        select id)
                .Distinct()
                .ToArray();
        }

		public static int[] ExtractRegionalProductIdsFromMarketing(int[] allIds, IArticleService articleService, int marketingProductContentId, string productsFieldName)
		{
			int[] marketingProductIds = GetIdsForContent(allIds, marketingProductContentId, articleService);

            var marketingProducts = articleService.List(marketingProductContentId, marketingProductIds);

			var regionalProductFromMarketingIds = from product in marketingProducts
											  from fv in product.FieldValues
											  where fv.Field.Name == productsFieldName
											  from id in fv.RelatedItems
											  select id;

			return allIds.Except(marketingProductIds).Concat(regionalProductFromMarketingIds).Distinct().ToArray();
		}

		public static int[] GetIdsForContent(int[] allIds, int contentId, IArticleService articleService)
		{
			return articleService.GetFieldValues(allIds, contentId, "Id").Select(int.Parse).ToArray();
		}
        
        public static int[] GetAllProductIds(int siteId, int productsContentId, Customer customer)
		{
			var dbConnector = customer.DbConnector;

			string siteName = dbConnector.GetSiteName(siteId);

			string contentName = dbConnector.GetContentName(productsContentId);

			long totalRecords = 0;

			var dtProducts = dbConnector.GetContentData(
				siteName,
				contentName,
				"CONTENT_ITEM_ID",
				null,
				null,
				0,
				int.MaxValue,
				out totalRecords,
				0,
				null,
				1,
				0);

			if (dtProducts.Rows.Count == 0)
				throw new Exception("No products found");

			return dtProducts.AsEnumerable().Select(x => (int)(decimal)x["CONTENT_ITEM_ID"]).ToArray();
		}

        public static Dictionary<int, int[]> GetContentIds(IEnumerable<int> ids, Customer customer)
        {
            var dbConnector = customer.DbConnector;
            var idList = SqlQuerySyntaxHelper.IdList(customer.DatabaseType, "@Ids", "Ids");
            var dbCommand = dbConnector.CreateDbCommand($@"SELECT CONTENT_ID, CONTENT_ITEM_ID 
					FROM CONTENT_ITEM 
					{SqlQuerySyntaxHelper.WithNoLock(customer.DatabaseType)} 
				WHERE CONTENT_ITEM_ID IN (SELECT ID FROM {idList})");
            dbCommand.Parameters.Add(SqlQuerySyntaxHelper.GetIdsDatatableParam("@Ids", ids, customer.DatabaseType));

            var dt = dbConnector.GetRealData(dbCommand);

            return dt
                .AsEnumerable()
                .GroupBy(x => (int) (decimal) x["CONTENT_ID"])
                .ToDictionary(x => x.Key, x => x.Select(y => (int) (decimal) y["CONTENT_ITEM_ID"]).ToArray());
        } 
    }
}
