using QA.ProductCatalog.Infrastructure;
using QA.Validation.Xaml;
using QA.Validation.Xaml.Extensions.Rules;
using QA.Validation.Xaml.ListTypes;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Mappers;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Models;
using Quantumart.QP8.BLL.Services.API;
using System;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using QA.Core.DPC.QP.Services;
using QA.Core.DPC.Resources;
using QA.ProductCatalog.ContentProviders;
using Quantumart.QP8.Constants;

namespace QA.ProductCatalog.Validation.Validators
{
	public class CommonMarketingProductValidator : IRemoteValidator2
	{
	    private readonly ISettingsService _service;
	    private readonly IConnectionProvider _provider;

        public CommonMarketingProductValidator(ISettingsService service, IConnectionProvider provider)
        {
            _provider = provider;
            _service = service;
        }

	    public RemoteValidationResult Validate(RemoteValidationContext model, RemoteValidationResult result)
		{
			var helper = new ValidationHelper(model, result, _provider, _service);

		    var productTypeName = _service.GetSetting(SettingsTitles.PRODUCT_TYPES_FIELD_NAME);



			using (new QPConnectionScope(helper.Customer.ConnectionString, (DatabaseType)helper.Customer.DatabaseType ))
			{
				var articleSerivce = new ArticleService(helper.Customer.ConnectionString, 1);
			    var emptyArticle = articleSerivce.New(model.ContentId);

			    var productsName = helper.GetRelatedFieldName(emptyArticle, helper.GetSettingValue(SettingsTitles.PRODUCTS_CONTENT_ID));
			    var marketingProductTypeId = helper.GetValue<int>(helper.GetClassifierFieldName(emptyArticle));
			    var productIds = helper.GetValue<ListOfInt>(productsName);

                if (productIds != null && AreTypesIncompatible(helper, articleSerivce, marketingProductTypeId, productIds, productTypeName))
                {
	                var message = new ActionTaskResultMessage()
	                {
		                ResourceClass = ValidationHelper.ResourceClass,
		                ResourceName = nameof(RemoteValidationMessages.SameTypeMarketingProductProducts),
	                };
					result.AddModelError(helper.GetPropertyName(productsName), helper.ToString(message));
					return result;
				}

				if (!GetUniqueAliasExclusions(helper).Contains(marketingProductTypeId))
				{
					var ids = CheckAliasUniqueness(helper, marketingProductTypeId, articleSerivce, productTypeName);
					if (!String.IsNullOrEmpty(ids))
					{
						var message = new ActionTaskResultMessage()
						{
							ResourceClass = ValidationHelper.ResourceClass,
							ResourceName = nameof(RemoteValidationMessages.MarketingProduct_Duplicate_Alias),
							Parameters = new object[] {ids}
						};
						
						result.AddModelError(
								helper.GetPropertyName(Constants.FieldAlias), helper.ToString(message)
						);
					}
				}
            }

		    return result;
        }

		private static string CheckAliasUniqueness(ValidationHelper helper, int marketingProductTypeId, ArticleService articleSerivce, string productTypeName)
		{
			string alias = helper.GetValue<string>(Constants.FieldAlias);
			int id = helper.GetValue<int>(Constants.FieldId);
			int marketingProductContentId = helper.GetSettingValue(SettingsTitles.MARKETING_PRODUCT_CONTENT_ID);
			helper.CheckSiteId(marketingProductContentId);

			var matchService = new ArticleMatchService<Expression<Predicate<IArticle>>>(
				helper.Customer.ConnectionString, helper.Customer.QpDatabaseType, new ExpressionConditionMapper()
			);
			object aliasValue = alias;
			var filter = $"c.{Constants.FieldAlias} = '{aliasValue}' and c.{productTypeName} = {marketingProductTypeId}";
			var matches = articleSerivce.List(marketingProductContentId, null, true, filter).Where(itm => itm.Id != id);
		    var ids = string.Join(", ", matches.Select( n => n.Id));
			return ids;
		}

		private static int[] GetUniqueAliasExclusions(ValidationHelper helper)
		{
			int[] marketingProductTypesToIgnoreAliasUniqueness = Enumerable.Empty<int>().ToArray();
			string marketingProductTypesToIgnoreAliasUniquenessSrt = null;

			try
			{
				marketingProductTypesToIgnoreAliasUniquenessSrt = helper.GetSettingStringValue(SettingsTitles.MARKETING_PRODUCT_TYPES_TO_IGNORE_ALIAS_UNIQUENESS);
			}
		    catch
		    {
		        // ignored
		    }

		    if (!string.IsNullOrEmpty(marketingProductTypesToIgnoreAliasUniquenessSrt))
				marketingProductTypesToIgnoreAliasUniqueness =
					marketingProductTypesToIgnoreAliasUniquenessSrt.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
						.Select(int.Parse)
						.ToArray();
			return marketingProductTypesToIgnoreAliasUniqueness;
		}

		private static bool AreTypesIncompatible(ValidationHelper helper, ArticleService articleSerivce, int marketingProductTypeId, ListOfInt productIds, string productTypeName)
		{

			bool typesIncompatible = false;
			
			int productContentId = helper.GetSettingValue(SettingsTitles.PRODUCTS_CONTENT_ID);

			var productTypeIds = articleSerivce.GetFieldValues(productIds.ToArray(), productContentId, productTypeName).Distinct().ToArray();

			if (productTypeIds.Length == 1)
			{
				string productTypeId = productTypeIds[0];

				int productTypesContentId = helper.GetSettingValue(SettingsTitles.PRODUCT_TYPES_CONTENT_ID);

				var allTypesCompatibility = articleSerivce.List(productTypesContentId, null, true);

				typesIncompatible = !allTypesCompatibility.Any(x =>
					x.FieldValues.FirstOrDefault(a => a.Field.Name == Constants.FieldProductContent)?.Value == productTypeId
					&&
					x.FieldValues.FirstOrDefault(a => a.Field.Name == Constants.FieldMarkProductContent)?.Value ==
					marketingProductTypeId.ToString());
			}
			else if (productTypeIds.Length > 1)
				typesIncompatible = true
					;
			return typesIncompatible;
		}
	}
}
