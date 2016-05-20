using QA.Core;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Validation.Resources;
using QA.Validation.Xaml;
using QA.Validation.Xaml.Extensions.Rules;
using QA.Validation.Xaml.Extensions.Rules.Remote;
using Quantumart.QP8.BLL.Services.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Quantumart.QP8.BLL;


namespace QA.ProductCatalog.Validation.Validators
{
	public class ValidationHelper 
	{

		public ValidationHelper(RemoteValidationContext model, ValidationContext result, string connectionStringName)
		{
			this.model = model;
			this.result = result;
			this.connectionStringName = connectionStringName;
			settingsService = ObjectFactoryBase.Resolve<ISettingsService>();
		}


		#region private
		private RemoteValidationContext model;
		private string connectionStringName;
		private ValidationContext result;
		private ISettingsService settingsService;
		#endregion


		public RemoteValidationContext Model
		{
			get { return model; }
		}

		public ValidationContext Result
		{
			get { return result; }
		}

		public RemotePropertyDefinition GetDefinition(string alias)
		{
			var def = model.Definitions.FirstOrDefault(x => x.Alias == alias);
			if (def == null)
			{
				result.AddErrorMessage(String.Format(RemoteValidationMessages.MissingParam, alias));
			}
			return def;
		}

		public string GetPropertyName(string alias)
		{
			return GetDefinition(alias).PropertyName;
		}

		public T GetValue<T>(string alias)
		{
			var def = GetDefinition(alias);
			return model.ProvideValueExact<T>(def);
		}

		public string ConnectionString
		{
			get
			{
				var connectinStringObject = ConfigurationManager.ConnectionStrings[connectionStringName];

				if (connectinStringObject == null)
					throw new ValidationException(string.Format(RemoteValidationMessages.EmptyConnectionString));

				return connectinStringObject.ConnectionString;

			}

		}

		public bool IsProductsRegionIntersectionsExists(ArticleService articleService, string productsName, int[] productsIds, string regionsName)
		{

			int productTypesContentId = GetSettingValue(SettingsTitles.PRODUCT_TYPES_CONTENT_ID);
			CheckSiteId(productTypesContentId);

			int contentId = GetSettingValue(SettingsTitles.PRODUCTS_CONTENT_ID);

			Dictionary<int, int[]> productToRegions = articleService.List(contentId, productsIds)
				  .ToDictionary<Article, int, int[]>(x => x.Id, x => x.FieldValues.Where(a => a.Field.Name == regionsName).Single().RelatedItems.ToArray());

			Dictionary<int, HashSet<int>> regionsToProducts = new Dictionary<int, HashSet<int>>();
			foreach (var item in productToRegions)
			{
				foreach (var regionId in item.Value)
				{
					if (!regionsToProducts.ContainsKey(regionId))
						regionsToProducts.Add(regionId, new HashSet<int>());

					if (!regionsToProducts[regionId].Contains(item.Key))
					{
						regionsToProducts[regionId].Add(item.Key);
					}
				}
			}
			
			var resultIds = productToRegions.Where(m => !m.Value.Select(n => regionsToProducts[n].Any(p => p != m.Key)).Any(n => n)).Select(m => m.Key);

			if (resultIds.Any())
			{
				result.AddModelError(GetPropertyName(productsName), string.Format(RemoteValidationMessages.Products_Different_Regions, String.Join(", ", resultIds)));
				return false;
			}

			return true;
		}

		public void CheckSiteId(int contentId)
		{
			var siteId = (new ContentService(ConnectionString, 1).Read(contentId)).SiteId;
			if (model.SiteId != siteId)
			{
				throw new ValidationException(RemoteValidationMessages.SiteIdInvalid);
			}

		}

		public int GetSettingValue(SettingsTitles key)
		{
			var valueStr = settingsService.GetSetting(key);
			int value = 0;
			if (string.IsNullOrEmpty(valueStr) || !int.TryParse(valueStr, out value))
			{
				throw new ValidationException(String.Format(RemoteValidationMessages.Settings_Missing, key));
			}
			return value;
		}


		public string GetSettingStringValue(SettingsTitles key)
		{
			var valueStr = settingsService.GetSetting(key);
			if (string.IsNullOrEmpty(valueStr))
			{
				throw new ValidationException(String.Format(RemoteValidationMessages.Settings_Missing, key));
			}
			return valueStr;
		}
	
	}
}