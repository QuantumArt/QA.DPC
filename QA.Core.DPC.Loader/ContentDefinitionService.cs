using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using QA.Configuration;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QPublishing.Database;
using QA.Core.Cache;
using Content = QA.Core.Models.Configuration.Content;
using QA.Core.Models.Configuration;
using System.Collections.Generic;
using Quantumart.QPublishing.Info;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.QP.Services;

namespace QA.Core.DPC.Loader
{
    public class ContentDefinitionService : IContentDefinitionService
	{
		#region Константы
		
		private const string FIELD_NAME_CONTENT = "Content";
		private const string FIELD_NAME_TITLE = "Title";
		private const string FIELD_NAME_XML_DEF = "XmlDefinition";
		private const string FIELD_NAME_APPLY_TO_TYPES = "ApplyToTypes";
		private const string FIELD_NAME_SLUG = "Slug";
		private const string FIELD_NAME_VERSION = "Version";
		private const string FIELD_NAME_DEFINITION = "Definition";
		private const string FIELD_NAME_TYPE = "Type";
		private const string FIELD_NAME_FILTER = "Filter";

		private const string KEY_GET_DEFINITION = "GetDefinition_{0}_{1}_{2}";
		#endregion

		#region Глобальные переменные
		private readonly ISettingsService _settingsService;
		private readonly IVersionedCacheProvider _cacheProvider;
		private readonly TimeSpan _cachePeriod = new TimeSpan(0, 10, 0);
		private readonly ILogger _logger;
		private readonly IArticleService _articleService;
		private readonly string _connectionString;

		#endregion

		#region Конструкторы
		public ContentDefinitionService(ISettingsService settingsService,
			IVersionedCacheProvider cacheProvider,
			IArticleService articleService,
			ILogger logger,
            IConnectionProvider connectionProvider)
		{
			_logger = logger;
			_settingsService = settingsService;
			_cacheProvider = cacheProvider;
			_articleService = articleService;
			_connectionString = connectionProvider.GetConnection(); 
		}
		#endregion

		#region IContentDefinitionService
		

		public Content GetDefinitionForContent(int productTypeId, int contentId, bool isLive = false)
		{
			return (Content)XamlConfigurationParser.CreateFrom(GetDefinitionXml(productTypeId, contentId, isLive));
		}

		public Content[] GetDefinitions(bool isLive = false)
		{
			int prodDefContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.PRODUCT_DEFINITIONS_CONTENT_ID));

			using (_articleService.CreateQpConnectionScope())
			{
				_articleService.IsLive = isLive;

				var definitions = _articleService.List(prodDefContentId, null).Where(x => !x.Archived && x.Visible);

				return definitions
					.Select(x => x.FieldValues
						.Where(a => a.Field.Name == FIELD_NAME_XML_DEF)
						.Select(a => a.Value)
						.FirstOrDefault())
					.Where(x => !string.IsNullOrEmpty(x))
					.Select(x => (Content)XamlConfigurationParser.CreateFrom(x))
					.ToArray();
			}
		}

		public ServiceDefinition GetServiceDefinition(string slug, string version, bool clearExtensions)
		{
			int productServicesContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.PRODUCT_SERVICES_CONTENT_ID));

			int prodDefContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.PRODUCT_DEFINITIONS_CONTENT_ID));

			string cacheKey = "KEY_GET_DEFINITION_BY_SLUG: " + slug + "_" + version;

			return _cacheProvider.GetOrAdd(cacheKey, new[] {productServicesContentId.ToString(), prodDefContentId.ToString()},
				_cachePeriod,
				() =>
				{
					using (_articleService.CreateQpConnectionScope())
					{
						var dbConnector = new DBConnector(_connectionString);

						string wherePart = string.Format("([{0}]='{1}' AND [{2}]='{3}')", FIELD_NAME_SLUG, slug.Replace("'", ""), FIELD_NAME_VERSION, version.Replace("'", ""));

						var dtdefinitionArticles = dbConnector.GetContentData(new ContentDataQueryObject(dbConnector, productServicesContentId, FIELD_NAME_DEFINITION + "," + FIELD_NAME_TYPE + "," + FIELD_NAME_FILTER, wherePart, null, 0, 1));

						if (dtdefinitionArticles.Rows.Count == 0)
							throw new Exception(string.Format("Slug '{0}' с версией '{1}' не найден", slug, version));

						int definitionArticleId = (int)(decimal)dtdefinitionArticles.Rows[0][FIELD_NAME_DEFINITION];

						object productTypeArticleId = dtdefinitionArticles.Rows[0][FIELD_NAME_TYPE];
						int[] exstensionContentIds = new int[0];

						var content = (Content)XamlConfigurationParser.CreateFrom(_articleService.GetFieldValues(new[] { definitionArticleId }, prodDefContentId, FIELD_NAME_XML_DEF)[0]);

						if (productTypeArticleId is decimal)
						{
							var productTypeArticle = _articleService.Read((int)(decimal)productTypeArticleId);
							exstensionContentIds = productTypeArticle.FieldValues
								.Where(fv => fv.Field.Name.EndsWith("content", StringComparison.CurrentCultureIgnoreCase) && fv.RelatedItems.Any())
								.SelectMany(fv => fv.RelatedItems)
								.Distinct()
								.ToArray();							
						}

						if (clearExtensions)
						{
							ClearExstensions(content, exstensionContentIds, null);
						}

						return new ServiceDefinition
						{
							Content = content,
							Filter = dtdefinitionArticles.Rows[0][FIELD_NAME_FILTER].ToString(),
							ExstensionContentIds = exstensionContentIds
						};
					}
				});
		}

		private void ClearExstensions(Content content, int[] exstensionContentIds, List<int> hashCodes)
		{
			int hashCode = content.GetHashCode();

			if (hashCodes == null)
			{
				hashCodes = new List<int> { hashCode };
			}
			else if (hashCodes.Contains(hashCode))
			{				
				return;
			}
			else
			{
				hashCodes.Add(hashCode);
			}

			var exstensions = content.Fields.OfType<ExtensionField>();

			foreach (var field in exstensions)
			{
				if (field.ContentMapping.Values.Any(c => exstensionContentIds.Contains(c.ContentId)))
				{
					int[] ids = field.ContentMapping.Where(e => !exstensionContentIds.Contains(e.Value.ContentId)).Select(e => e.Key).ToArray();
					
					foreach (int id in ids)
					{
						field.ContentMapping.Remove(id);
					}
				}

				foreach (var c in field.ContentMapping.Values)
				{
					ClearExstensions(c, exstensionContentIds, hashCodes);
				}
			}

			foreach (var field in content.Fields.OfType<EntityField>())
			{
				ClearExstensions(field.Content, exstensionContentIds, hashCodes);			
			}		
		}

		public string GetControlDefinition(int contentId, int productTypeId)
		{
			_logger.Debug("Запрошен контрол для contentId: {0},  productTypeId: {1}", contentId, productTypeId);

			//return ResourceHelper.GetXaml<Content>(string.Format("QA.Core.DPC.Loader.Xaml.content_{0}.xaml", contentId));
			int prodDefContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.PRODUCT_CONTROL_CONTENT_ID));

			string key = "KEY_GET_DEFINITION_CONTROL" + productTypeId + contentId;

			return _cacheProvider.GetOrAdd(key, new[] { prodDefContentId.ToString() }, _cachePeriod, () =>
			{
				using (_articleService.CreateQpConnectionScope())
				{
					var definitions = _articleService.List(prodDefContentId, null).Where(x => !x.Archived);

					if (contentId > 0)
					{
						definitions = definitions.Where(x => x.FieldValues.Any(a => a.Field.Name == FIELD_NAME_CONTENT && a.RelatedItems.Contains(contentId)));
					}

					if (productTypeId > 0)
					{
						var prodTypesContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.PRODUCT_TYPES_CONTENT_ID));

						var types = _articleService.List(prodTypesContentId, null).Where(x => !x.Archived && x.Visible); //Все типы продуктов
						//Идентификаторы статей в "Типы продуктов", подходящих данному productTypeId. Т.е. productTypeId есть либо в поле MarketingProductContent либо в поле ProductContent
						var typesIds = types.Where(x => x.FieldValues.Any(a => a.Field.Name != FIELD_NAME_TITLE && a.Value == productTypeId.ToString())).Select(x => x.Id).ToList();
						definitions = definitions.Where(x => x.FieldValues.Any(a => a.Field.Name == FIELD_NAME_APPLY_TO_TYPES && a.RelatedItems.Intersect(typesIds).Any()));
					}

					var res = definitions.Select(x => x.FieldValues.Where(a => a.Field.Name == FIELD_NAME_XML_DEF).Select(a => a.Value).FirstOrDefault()).FirstOrDefault();

					if (string.IsNullOrEmpty(res))
					{
						return null;
					}

					return res; //ResourceHelper.GetXaml<Content>(res);
				}
			});
		}
		

		#endregion

		
		public string GetDefinitionXml(int productTypeId, int contentId, bool isLive = false)
		{
			//return ResourceHelper.GetXaml<Content>(string.Format("QA.Core.DPC.Loader.Xaml.content_{0}.xaml", contentId));
			int prodDefContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.PRODUCT_DEFINITIONS_CONTENT_ID));

			string keyInCache = string.Format(KEY_GET_DEFINITION, productTypeId, contentId, isLive);

			return _cacheProvider.GetOrAdd(keyInCache, new[] { prodDefContentId.ToString() }, _cachePeriod, () =>
			{
				using (_articleService.CreateQpConnectionScope())
				{
					_articleService.IsLive = isLive;

					var definitions = _articleService.List(prodDefContentId, null).Where(x => !x.Archived && x.Visible);

					if (contentId > 0)
					{
						definitions = definitions.Where(x => x.FieldValues.Any(a => a.Field.Name == FIELD_NAME_CONTENT && a.RelatedItems.Contains(contentId)));
					}

					if (productTypeId > 0)
					{
						int prodTypesContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.PRODUCT_TYPES_CONTENT_ID));

						var types = _articleService.List(prodTypesContentId, null).Where(x => !x.Archived && x.Visible); //Все типы продуктов
						//Идентификаторы статей в "Типы продуктов", подходящих данному productTypeId. Т.е. productTypeId есть либо в поле MarketingProductContent либо в поле ProductContent
						var typesIds = types.Where(x => x.FieldValues.Any(a => a.Field.Name != FIELD_NAME_TITLE && a.Value == productTypeId.ToString())).Select(x => x.Id).ToList();
						definitions = definitions.Where(x => x.FieldValues.Any(a => a.Field.Name == FIELD_NAME_APPLY_TO_TYPES && a.RelatedItems.Intersect(typesIds).Any()));
					}

					string res = definitions.Select(x => x.FieldValues.Where(a => a.Field.Name == FIELD_NAME_XML_DEF).Select(a => a.Value).FirstOrDefault()).FirstOrDefault();

					if (string.IsNullOrEmpty(res))
						return null;

					return res; 
				}
			});
		}

		public string GetDefinitionXml(int articleId, bool isLive = false)
		{
			_articleService.IsLive = isLive;

			var article = _articleService.Read(articleId);

			return article.FieldValues.Single(x => x.Field.Name == FIELD_NAME_XML_DEF).Value;
		}

		public void SaveDefinition(int content_item_id, string xml)
		{
			using (_articleService.CreateQpConnectionScope())
			{
				var article = _articleService.Read(content_item_id);

				article.FieldValues.Single(x => x.Field.Name == FIELD_NAME_XML_DEF).Value = xml;

				_articleService.Save(article);
			}
		}

		#region Закрытые методы
		#region nested
		internal static class ResourceHelper
		{
			public static T GetXaml<T>(string path)
			{
				using (var stream = Assembly.GetExecutingAssembly()
				   .GetManifestResourceStream(path))
				{
					// создаем экземпляр валидатора
					return (T)XamlConfigurationParser.CreateFrom(stream);
				}
			}

			public static string GetEmbeddedResourceText(string path)
			{
				using (var stream = Assembly.GetExecutingAssembly()
				   .GetManifestResourceStream(path))
				{
					using (var textReader = new StreamReader(stream))
					{
						return textReader.ReadToEnd();
					}
				}
			}
		}
		#endregion
		#endregion
	}
}
