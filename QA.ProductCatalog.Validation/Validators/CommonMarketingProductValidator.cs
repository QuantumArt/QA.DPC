using QA.ProductCatalog.Infrastructure;
using QA.Validation.Xaml;
using QA.Validation.Xaml.Extensions.Rules;
using QA.Validation.Xaml.ListTypes;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository.ArticleMatching;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Mappers;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Models;
using Quantumart.QP8.BLL.Services.API;
using System;
using System.Linq;
using System.Linq.Expressions;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Validation.Resources;

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



			using (new QPConnectionScope(helper.ConnectionString))
			{
				var articleSerivce = new ArticleService(helper.ConnectionString, 1);
			    var emptyArticle = articleSerivce.New(model.ContentId);

			    var productsName = helper.GetRelatedFieldName(emptyArticle, helper.GetSettingValue(SettingsTitles.PRODUCTS_CONTENT_ID));
			    var marketingProductTypeId = helper.GetValue<int>(helper.GetClassifierFieldName(emptyArticle));
			    var productIds = helper.GetValue<ListOfInt>(productsName);

                if (productIds != null && AreTypesIncompatible(helper, articleSerivce, marketingProductTypeId, productIds, productTypeName))
				{
					result.AddModelError(helper.GetPropertyName(productsName), RemoteValidationMessages.SameTypeMarketingProductProducts);
					return result;
				}

				if (!GetUniqueAliasExclusions(helper).Contains(marketingProductTypeId))
				{
					var ids = CheckAliasUniqueness(helper, marketingProductTypeId, articleSerivce, productTypeName);
					if (!String.IsNullOrEmpty(ids))
					{
						result.AddModelError(
							helper.GetPropertyName(Constants.FieldAlias),
							string.Format(RemoteValidationMessages.MarketingProduct_Duplicate_Alias, ids)
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

			var matchService = new ArticleMatchService<Expression<Predicate<IArticle>>>(helper.ConnectionString, new ExpressionConditionMapper());
			object aliasValue = alias;
			var matchItems = matchService.MatchArticles(marketingProductContentId, article => article[Constants.FieldAlias].Value == aliasValue, MatchMode.Strict);
			var matchIds = matchItems.Where(itm => itm.Id != id).Select(itm => itm.Id).ToArray();

		    var ids = "";
		    var matches = articleSerivce.List(marketingProductContentId, matchIds, true).ToArray();
		    if (matches.Any())
		    {
		        var fieldValues = matches.Select(n => n.FieldValues.Single(m => m.Field.Name == productTypeName)).ToArray();
		        ids = String.Join(", ", fieldValues.Where(n => int.Parse(n.Value) == marketingProductTypeId).Select(n => n.Article.Id));
		    }
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
