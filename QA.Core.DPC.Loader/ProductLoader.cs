using System.Data;
using System.Data.SqlClient;
using QA.Core.Models.Entities;
using Quantumart.QPublishing.Database;
using Qp8Bll = Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API;
using System;
using System.Collections.Generic;
using System.Linq;
using QA.Core.Models.Configuration;
using QA.ProductCatalog.Infrastructure;
using QA.Core.DPC.Loader.Resources;
using System.Configuration;
using QA.Core.Cache;
using System.Diagnostics;
using QA.Core.DPC.Loader.Services;
using QA.Core.Models.Filters;
using QA.Core.Models.UI;
using QA.Core.ProductCatalog.Actions.Services;
using Quantumart.QP8.Utils;
using System.Xml.Linq;
using Quantumart.QPublishing.Info;
using Content = QA.Core.Models.Configuration.Content;

namespace QA.Core.DPC.Loader
{
	public class ProductLoader : IProductService
	{
		#region Константы
		private const string ARTICLE_STATUS_PUBLISHED = "Published";
		public const string KEY_CONNECTION_STRING = "qp_database";

		private const string KEY_CACHE_GET_ARTICLE = "GetArticle_";

		#endregion

		#region private fields
		private readonly IContentDefinitionService _definitionService;
		private readonly ILogger _logger;
		private int _hits;
		private int _misses;

        private readonly IVersionedCacheProvider _cacheProvider;
		private readonly ICacheItemWatcher _cacheItemWatcher;

		private readonly string _connectionString;
		private readonly IFieldService _fieldService;
		private readonly ISettingsService _settingsService;
		private readonly IConsumerMonitoringService _consumerMonitoringService;

		private IReadOnlyArticleService _articleService { get; set; }

		#endregion

		#region Конструкторы
		public ProductLoader(IContentDefinitionService definitionService, ILogger logger, 
            IVersionedCacheProvider cacheProvider, ICacheItemWatcher cacheItemWatcher, 
            IReadOnlyArticleService articleService, IFieldService fieldService, ISettingsService settingsService, 
            IConsumerMonitoringService consumerMonitoringService)
		{
			_definitionService = definitionService;
			_logger = logger;
			_cacheProvider = cacheProvider;
			_cacheItemWatcher = cacheItemWatcher;
			_articleService = articleService;
			_fieldService = fieldService;
			_settingsService = settingsService;
			_consumerMonitoringService = consumerMonitoringService;

			var connectinStringObject = ConfigurationManager.ConnectionStrings[KEY_CONNECTION_STRING];
			if (connectinStringObject == null)
			{
				throw new Exception(string.Format(ProductLoaderResources.ERR_CONNECTION_STRING_NO_EXISTS, KEY_CONNECTION_STRING));
			}
			_connectionString = connectinStringObject.ConnectionString;
		}
		#endregion

		#region IProductService

		public virtual Dictionary<string, object>[] GetProductsList(ServiceDefinition definition, long startRow, long pageSize, bool isLive)
		{
			var fields = _fieldService.List(definition.Content.ContentId).ToArray();

			var fieldNames = definition.Content.Fields
				.Where(x => x is PlainField && ((PlainField)x).ShowInList)
				.Select(x => fields.Single(y => y.Id == x.FieldId).Name)
				.ToList();

			fieldNames.Add("CONTENT_ITEM_ID");

			using (new Quantumart.QP8.BLL.QPConnectionScope(_connectionString))
			{
				var dbConnector = new DBConnector(_connectionString);

				var dtdefinitionArticles =
					dbConnector.GetContentData(
					new ContentDataQueryObject(
						dbConnector,
						definition.Content.ContentId,
						string.Join(",", fieldNames),
						definition.Filter,
						null,
						startRow,
						pageSize) { ShowSplittedArticle = (byte)(isLive ? 0 : 1) });

				return
					dtdefinitionArticles
						.AsEnumerable()
						.Select(x => fieldNames
									.Where(y => x[y] != DBNull.Value)
									.ToDictionary(y => y == "CONTENT_ITEM_ID" ? "id" : y, y => y == "CONTENT_ITEM_ID" || fields.Single(z => z.Name == y).IsInteger ? (int)(decimal)x[y] : x[y]))
						.ToArray();
			}
		}


		/// <summary>
		/// Получение структурированного продукта на основе XML с маппингом данных
		/// </summary>
		/// <param name="id">Идентификатор продукта, по которому надо сделать мапинг</param>
		/// <returns></returns>
		public virtual Article GetProductById(int id, bool isLive = false, ProductDefinition productDefinition = null)
		{
			using (new Qp8Bll.QPConnectionScope(_connectionString))
			{
				_articleService.IsLive = isLive;

				this._cacheItemWatcher.TrackChanges();

				var timer = new Stopwatch();

				timer.Start();

				_articleService.LoadStructureCache();

				timer.Stop();

				_logger.Debug("LoadStructureCache took {0} secs", timer.Elapsed.TotalSeconds);


				// вот тут мы принудительно проверяем изменения и чистим кеш.

				timer.Stop();

				Article article = null;

				if (productDefinition != null)
				{
					string keyInCache = GetArticleKeyStringForCache(new ArticleShapedByDefinitionKey(id, productDefinition.StorageSchema, isLive));

					article = (Article)_cacheProvider.Get(keyInCache);
				}

				if (article == null)
				{
                    var counter = GetArticleCounter(id);

                    var qpArticle = ReadArticle(id, isLive);

					if (qpArticle == null)
						return null;

					int productTypeId; // Идентификатор контента-расширения

					int.TryParse(qpArticle.FieldValues.Where(x => x.Field.IsClassifier).Select(x => x.Value).FirstOrDefault(),
						out productTypeId);

					if (productDefinition == null)
						productDefinition = GetProductDefinition(productTypeId, qpArticle.ContentId, isLive);

					timer.Reset();

					timer.Start();

					article = GetProduct(qpArticle, productDefinition, isLive, counter);
                    counter.LogCounter();
                }

				if (article.HasVirtualFields)
					article = GenerateArticleWithVirtualFields(article, productDefinition.StorageSchema);

				timer.Stop();

				_logger.Debug("GetProduct took {0} secs", timer.Elapsed.TotalSeconds);

				return article;
			}
		}

		public virtual Article[] GetProductsByIds(int[] ids, bool isLive = false)
		{
			var dbConnector = new DBConnector(_connectionString);

			var sqlCommand = new SqlCommand(@"SELECT CONTENT_ID, CONTENT_ITEM_ID FROM CONTENT_ITEM WITH(NOLOCK) WHERE CONTENT_ITEM_ID IN (SELECT ID FROM @Ids)");

			sqlCommand.Parameters.Add(Common.GetIdsTvp(ids, "@Ids"));

			var dt = dbConnector.GetRealData(sqlCommand);

			return dt
				.AsEnumerable()
				.GroupBy(x => (int)(decimal)x["CONTENT_ID"])
				.SelectMany(x => GetProductsByIds(x.Key, x.Select(y => (int)(decimal)y["CONTENT_ITEM_ID"]).ToArray(), isLive))
				.ToArray();
		}

		public virtual Article[] GetProductsByIds(int contentId, int[] ids, bool isLive = false)
		{
			var res = new List<Article>();
			using (new Qp8Bll.QPConnectionScope(_connectionString))
			{
				_articleService.IsLive = isLive;

				_cacheItemWatcher.TrackChanges();

				_articleService.LoadStructureCache();
                
				var articles = _articleService.List(contentId, ids);

				if (articles == null || !articles.Any())
					return new Article[] { };

				//Группируем статьи по значению productTypeId (идентификатору контента-расширения)
				var groupedArticles = from article in articles
									  group article by article.FieldValues.Where(a => a.Field.IsClassifier).Select(a => a.Value).FirstOrDefault() into gr
									  select gr;

				foreach (var group in groupedArticles)
				{
					var productTypeId = int.Parse(group.Key ?? "0");

					ProductDefinition productDefinition = GetProductDefinition(productTypeId, contentId, isLive);

					// это кеш загруженных статей в рамках одного запроса (подходит для тупого кеширования в лоб словарей)
					// по идее словари надо предзагружать и следить за их изменением
					if (productDefinition == null)
						throw new Exception(string.Format("Для данныхй статей {0} не определено описание продукта", string.Join(", ", ids)));
                   
                    var loadedArticles = InitDictionaries(productDefinition, isLive, GetDictionaryCounter());

					res.AddRange(
						group
							.Select(product =>
							{
                                var counter = GetArticleCounter(product.Id);
                                var article = GetArticle(product, productDefinition.StorageSchema, loadedArticles, isLive, counter);

								if (article != null && article.HasVirtualFields)
									article = GenerateArticleWithVirtualFields(article, productDefinition.StorageSchema);

                                counter.LogCounter();
                                return article;
							})
							.Where(article => article != null));
				}
			}

			return res.ToArray();
		}

		public virtual Article[] GetSimpleProductsByIds(int[] ids, bool isLive = false)
		{
			var products = new List<Article>();
			var items = GetProductsInfo(ids);
			var existingItems = items.Where(itm => itm.ContentName != null).ToArray();
			var missingIds = items.Where(itm => itm.ContentName == null).Select(itm => itm.ProductId).ToArray();

			if (missingIds.Any())
			{
				var idsToСlarify = _consumerMonitoringService.FindExistingProducts(missingIds);
				products.AddRange(missingIds.Select(id => GetMissingProduct(idsToСlarify, id)));
			}

			products.AddRange(existingItems.Select(item => GetExistingProduct(item)));

			return products.ToArray();
	
		}

		private static Article GetExistingProduct(ProductInfo item)
		{
			Article product = null;

			product = new Article
			{
				Id = item.ProductId,
				Visible = true,
				Archived = false,
				IsPublished = true,
				ContentId = item.ContentId,
				ContentName = item.ContentName,
				ContentDisplayName = item.ContentDisplayName
			};

			if (!String.IsNullOrEmpty(item.ExtensionAttributeName))
			{
				product.Fields.Add(item.ExtensionAttributeName, new ExtensionArticleField
				{
                    ContentId = item.ExtensionContentId,
					Value = item.ExtensionContentId.ToString(),
					FieldName = item.ExtensionAttributeName,
					Item = new Article
					{
                        Visible = true,
						ContentId = item.ExtensionContentId,
						ContentName = item.ExtensionName,
						ContentDisplayName = item.ExtensionDisplayName
					}
				});
			}

			return product;
		}

		private Article GetMissingProduct(int[] idsToСlarify, int id)
		{
			string typeField = _settingsService.GetSetting(SettingsTitles.PRODUCT_TYPES_FIELD_NAME);

			Article product = null;

			product = new Article
			{
				Id = id,
				ContentId = 0,
				Visible = true,
				Archived = false,
				ContentName = string.Empty,
				IsPublished = true
			};

			if (idsToСlarify.Contains(id))
			{
				var xml = _consumerMonitoringService.GetProductXml(id);
				XDocument doc = null;
				try
				{
					doc = XDocument.Parse(xml);
				}
				catch (Exception)
				{
					// бинарная витрина
				}

				var typeEl = doc?.Descendants("Product").Elements().FirstOrDefault(n => n.Name.LocalName == typeField);


				if (typeEl != null)
				{
					product.Fields.Add(typeField, new ExtensionArticleField
					{
						ContentId = 0,
						FieldName = typeField,
						Item = new Article
						{
							Visible = true,
							ContentId = 0,
							ContentName = typeEl.Value
						}
					});
				}
			}

			return product;
		}

		/// <summary>
		/// Получение структуры данных на основе XML с мапингом данных из БД 
		/// </summary>
		/// <param name="productTypeId">Идентификатор типа продукта</param>
		/// <returns></returns>
		public virtual ProductDefinition GetProductDefinition(int productTypeId, bool isLive = false)
		{
			var res = new ProductDefinition
			{
				ProdictTypeId = productTypeId,
				StorageSchema = _definitionService.GetDefinitionForContent(productTypeId, 0, isLive)
			};

			return res;
		}

		public virtual ProductDefinition GetProductDefinition(int productTypeId, int contentId, bool isLive = false)
		{
			ProductDefinition res = new ProductDefinition()
			{
				ProdictTypeId = productTypeId
			};

			res.StorageSchema = _definitionService.GetDefinitionForContent(productTypeId, contentId, isLive);

			if (res.StorageSchema == null)
				throw new Exception(string.Format("Не найден definition productTypeId={0} contentId={1} isLive={2}", productTypeId, contentId, isLive));

			return res;
		}
		#endregion

		#region Закрытые методы

		#region Queries
		private const string GetProductTypesQuery = @"SELECT
	ids.Id [ProductId],
	c.CONTENT_ID [ContentId],
	c.NET_CONTENT_NAME [ContentName],
	c.CONTENT_NAME [ContentDisplayName],
	itm.VISIBLE Visible,
	itm.ARCHIVE Archive,
	cl.ATTRIBUTE_NAME ExtensionAttributeName,
	ec.CONTENT_ID [ExtensionContentId],
	ec.NET_CONTENT_NAME [ExtensionName],
	ec.CONTENT_NAME [ExtensionDisplayName]
FROM
	@ids ids
	LEFT JOIN CONTENT_ITEM itm ON ids.Id = itm.CONTENT_ITEM_ID
	LEFT JOIN Content c ON itm.CONTENT_ID = c.CONTENT_ID
	LEFT JOIN (select content_id, attribute_id, attribute_name from CONTENT_ATTRIBUTE where is_classifier = 1) cl on c.CONTENT_ID = cl.CONTENT_ID
	LEFT JOIN CONTENT_DATA cd on cd.CONTENT_ITEM_ID = itm.CONTENT_ITEM_ID and cd.ATTRIBUTE_ID = cl.ATTRIBUTE_ID
	LEFT JOIN content ec on cd.DATA = ec.CONTENT_ID";

		public ProductInfo[] GetProductsInfo(int[] productIds)
		{
			using (var cs = new Quantumart.QP8.BLL.QPConnectionScope(ConfigurationManager.ConnectionStrings[ProductLoader.KEY_CONNECTION_STRING].ConnectionString))
			{
				var cnn = new DBConnector(cs.DbConnection);
				using (var cmd = new SqlCommand(GetProductTypesQuery))
				{
					cmd.CommandType = CommandType.Text;
					cmd.Parameters.Add(new SqlParameter("@ids", SqlDbType.Structured) { TypeName = "Ids", Value = Quantumart.QP8.DAL.Common.IdsToDataTable(productIds) });
					var data = cnn.GetRealData(cmd);
					return data.AsEnumerable().Select(row => Converter.ToModelFromDataRow<ProductInfo>(row)).ToArray();
				}
			}
		}

		public class ProductInfo
		{
			public int ProductId { get; set; }
			public int ContentId { get; set; }
			public string ContentName { get; set; }
			public string ContentDisplayName { get; set; }
			public bool Visible { get; set; }
			public bool Archive { get; set; }
			public string ExtensionAttributeName { get; set; }
			public int ExtensionContentId { get; set; }
			public string ExtensionName { get; set; }
			public string ExtensionDisplayName { get; set; }
		}
		#endregion

		#region IProductService
		/// <summary>
		/// Получение продукта. _articleService и _fieldService должны быть уже заполнены
		/// </summary>
		private Article GetProduct(Qp8Bll.Article qpArticle, ProductDefinition productDefinition, bool isLive, ArticleCounter counter)
		{
			var stopWatch = new Stopwatch();

			stopWatch.Start();

			var loadedArticles = InitDictionaries(productDefinition, isLive, GetDictionaryCounter());
            

            stopWatch.Stop();

			_logger.Info(string.Format("InitDictionaries called with hits {0} misses {1}, took {2} secs", _hits, _misses, stopWatch.Elapsed.TotalSeconds));

			_hits = _misses = 0;

            var article = GetArticle(qpArticle, productDefinition.StorageSchema, loadedArticles, isLive, counter);

			_logger.Info(string.Format("GetArticle called with hits {0} misses {1}", _hits, _misses));

			return article;
		}

		private Article GenerateArticleWithVirtualFields(Article article, Content content)
		{
			var articleCopy = ArticleFilteredCopier.Copy(article, new NullFilter());

			FillVirtualFields(articleCopy, content, new HashSet<Tuple<int, Content>>());

			return articleCopy;
		}

		private void FillVirtualFields(Article article, Content content, HashSet<Tuple<int, Content>> processedArticleKeys)
		{
			var articleToProcessKey = new Tuple<int, Content>(article.Id, content);

			if (processedArticleKeys.Contains(articleToProcessKey))
				return;

			processedArticleKeys.Add(articleToProcessKey);

			foreach (ArticleField articleField in article.Fields.Values)
			{
				if (articleField is SingleArticleField)
				{
					var singleArticleField = (SingleArticleField)articleField;

					if (singleArticleField.Item != null)
					{
						var field = content.Fields.Single(x => x.FieldId == articleField.FieldId);

						var childContent = field is EntityField
							? ((EntityField)field).Content
							: ((ExtensionField)field).ContentMapping[singleArticleField.Item.ContentId];

						FillVirtualFields(singleArticleField.Item, childContent, processedArticleKeys);
					}
				}
				else if (articleField is MultiArticleField)
				{
					var multiArticleField = (MultiArticleField)articleField;

					var field = content.Fields.Single(x => x.FieldId == articleField.FieldId);

					var childContent = ((EntityField)field).Content;

					foreach (var childArticle in multiArticleField.Items.Values)
						FillVirtualFields(childArticle, childContent, processedArticleKeys);
				}
			}

			foreach (BaseVirtualField virtualField in content.Fields.Where(x => x is BaseVirtualField))
			{
				var field = CreateAnyTypeVirtualField(virtualField, article);

				if (field != null)
					article.Fields.Add(field.FieldName, field);
			}
		}

		private ArticleField CreateAnyTypeVirtualField(BaseVirtualField virtualField, Article article)
		{
			ArticleField field;

			if (virtualField is VirtualField)
				field = CreateVirtualField((VirtualField) virtualField, article);
			else if (virtualField is VirtualMultiEntityField)
				field = CreateVirtualMiltiEntityField((VirtualMultiEntityField) virtualField, article);
			else if (virtualField is VirtualEntityField)
				field = CreateVirtualArticleField((VirtualEntityField) virtualField, article);
			else throw new Exception(string.Format("Виртуальное поле типа {0} не поддерживается", virtualField.GetType()));

			return field;
		}

		private ArticleField CreateVirtualMiltiEntityField(VirtualMultiEntityField virtualMultiEntityField, Article article)
		{
			var articlesWithParents = FilterableBindingValueProvider.EvaluatePath(virtualMultiEntityField.Path, article);

			if (!articlesWithParents.Any())
				return null;

			if(articlesWithParents.Any(x => !(x.ModelObject is Article)))
				throw new Exception("VirtualMultiEntityField требует что бы в Path был фильтр по Article");

			foreach (var articleWithParent in articlesWithParents)
				RemoveModelObject(articleWithParent);

			return new VirtualMultiArticleField
			{
				VirtualArticles = articlesWithParents
					.Select(x => CreateVirtualArticleField(virtualMultiEntityField, (Article) x.ModelObject))
					.Where(x => x != null)
					.ToArray(),
				FieldName = virtualMultiEntityField.FieldName
			};
		}

		private VirtualArticleField CreateVirtualArticleField(VirtualEntityField virtualEntityField, Article article)
		{
			var fields = virtualEntityField.Fields
				.Select(x => CreateAnyTypeVirtualField(x, article))
				.Where(x => x != null)
				.ToArray();

			return fields.Any()
				? new VirtualArticleField { FieldName = virtualEntityField.FieldName, Fields = fields }
				: null;
		}

		private ArticleField CreateVirtualField(VirtualField virtualField, Article article)
		{
			var foundObjectWithParent = FilterableBindingValueProvider.EvaluatePath(virtualField.Path, article).FirstOrDefault();

			if (!virtualField.PreserveSource)
			{
				if (!string.IsNullOrEmpty(virtualField.ObjectToRemovePath))
				{
					var objectToRemove = FilterableBindingValueProvider.EvaluatePath(virtualField.ObjectToRemovePath, article).FirstOrDefault();

					if (objectToRemove != null)
						RemoveModelObject(objectToRemove);
				}

				if (foundObjectWithParent != null)
					RemoveModelObject(foundObjectWithParent);
			}

			var foundObject = foundObjectWithParent == null ? null : foundObjectWithParent.ModelObject;

			var covertedObject = virtualField.Converter == null ? foundObject : virtualField.Converter.Convert(foundObject);

			if (covertedObject == null)
				return null;

			if (covertedObject is ArticleField)
			{
				var field = (ArticleField)covertedObject;

				field.FieldName = virtualField.FieldName;

				return field;
			}
			else if (covertedObject is Article)
			{
				var foundArticle = (Article)covertedObject;

				return new SingleArticleField {Item = foundArticle, FieldName = virtualField.FieldName};
			}
			else
				return new PlainArticleField
				{
					FieldName = virtualField.FieldName,
					NativeValue = covertedObject,
					Value = covertedObject.ToString()
				};
		}

		private void RemoveModelObject(FilterableBindingValueProvider.ModelObjectWithParent modelObjectWithParent)
		{
			if (modelObjectWithParent.ModelObject is ArticleField)
				((Article)modelObjectWithParent.Parent).Fields.Remove(((ArticleField)modelObjectWithParent.ModelObject).FieldName);
			else if (modelObjectWithParent.ModelObject is Article)
				((MultiArticleField)modelObjectWithParent.Parent).Items.Remove(((Article)modelObjectWithParent.ModelObject).Id);
		}

		#endregion

		#region Чтение на основе XML
		/// <summary>
		/// Получение статьи и всех ее полей, описанных в структуре маппинга
		/// </summary>
		/// <param name="id">Идентификатор статьи</param>
		/// <param name="contentDef">Структура маппинга</param>
		/// <param name="dictionaries">Словари, полученные на основе стуктуры XML</param>
		/// <param name="subContentIds">Словарь зависимостей контентов (по всем уровням). key=contentId</param>
		/// <returns></returns>
		private Article GetArticle(int id, Content contentDef, Dictionary<ArticleShapedByDefinitionKey, Article> loadedArticlesCache, bool isLive, ArticleCounter counter)
		{
			if (id == default(int))
				return null;

			if (contentDef == null)
				throw new Exception(string.Format(ProductLoaderResources.ERR_XML_CONTENT_MAP_NOT_EXISTS, id));

            var keyInLoadedArticlesCache = new ArticleShapedByDefinitionKey(id, contentDef, isLive);

			Article res;

			if (loadedArticlesCache.TryGetValue(keyInLoadedArticlesCache, out res))
			{
				_hits += 1;
                counter.CheckCacheArticlesLimit(id);
                return res;
			}


			var cachePeriod = contentDef.GetCachePeriodForContent();

			if (cachePeriod != null)
			{
				string[] cacheTags = GetTags(contentDef);

				string key = GetArticleKeyStringForCache(new ArticleShapedByDefinitionKey(id, contentDef, isLive));

				return _cacheProvider.GetOrAdd(key, cacheTags, cachePeriod.Value, () =>
				{
					var qpArticle = ReadArticle(id, isLive, contentDef);

					return GetArticle(qpArticle, contentDef, loadedArticlesCache, isLive, counter);
				});
			}

			var article = ReadArticle(id, isLive, contentDef);
            
			return GetArticle(article, contentDef, loadedArticlesCache, isLive, counter);
		}

        /// <summary>
        /// Получение статьи и всех ее полей, описанных в структуре маппинга
        /// </summary>
        /// <param name="id">Идентификатор статьи</param>
        /// <param name="contentDef">Структура маппинга</param>
        /// <param name="dictionaries">Словари, полученные на основе стуктуры XML</param>
        /// <param name="subContentIds">Словарь зависимостей контентов (по всем уровням). key=contentId</param>
        /// <returns></returns>
        private Article GetArticle(Qp8Bll.Article article, Content contentDef, Dictionary<ArticleShapedByDefinitionKey, Article> loaded, bool isLive, ArticleCounter counter)
		{
            if (article == null)
				return null;

            counter.CheckHitArticlesLimit(article.Id);

            if (isLive && article.Status.Name != ARTICLE_STATUS_PUBLISHED)
			{
				return null;
			}

			var cachePeriod = contentDef.GetCachePeriodForContent();

			if (cachePeriod != null)
			{
				string[] cacheTags = GetTags(contentDef);

				string key = GetArticleKeyStringForCache(new ArticleShapedByDefinitionKey(article.Id, contentDef, isLive));

				return _cacheProvider.GetOrAdd(key, cacheTags, cachePeriod.Value, () => GetArticleNotCached(article, contentDef, loaded, isLive, counter));
			}
			else
			{
				return GetArticleNotCached(article, contentDef, loaded, isLive, counter);
			}
		}


		private Article GetArticleNotCached(Qp8Bll.Article article, Content contentDef, Dictionary<ArticleShapedByDefinitionKey, Article> loadedArticlesCache, bool isLive, ArticleCounter counter)
		{
			if (article == null)
				throw new Exception(ProductLoaderResources.ERR_XML_ARTICLE_NOT_EXISTS);

			if (contentDef == null)
				throw new Exception(string.Format(ProductLoaderResources.ERR_XML_CONTENT_MAP_NOT_EXISTS, article.Id));

			var keyInLoaded = new ArticleShapedByDefinitionKey(article.Id, contentDef, isLive);

			Article res;

			if (loadedArticlesCache.TryGetValue(keyInLoaded, out res))
			{
				_hits += 1;
				return res;
			}
			else
				_misses += 1;

			res = new Article
			{
				ContentId = contentDef.ContentId,
				Archived = article.Archived,
				ContentDisplayName = article.DisplayContentName,
				PublishingMode = contentDef.PublishingMode,
				ContentName = article.Content.NetName,
				Created = article.Created,
				Modified = article.Modified,
				IsPublished = article.Status.Name == ARTICLE_STATUS_PUBLISHED,
				Splitted = article.Splitted,
				Status = article.Status.Name,
				Visible = article.Visible,
				Id = article.Id,
				HasVirtualFields = contentDef.Fields.Any(x => x is BaseVirtualField)
			};

			//кладем в словарь Article до а не после загрузки полей так как может быть цикл по данным одновременно с циклом по дефинишенам
			//и иначе бы был stackowerflow на вызовах GetArticleNotCached->GetArticleField->GetArticleNotCached->...
			loadedArticlesCache[keyInLoaded] = res;

			//Заполнение Plain-полей по параметру LoadAllPlainFields="True"  
			if (contentDef.LoadAllPlainFields)
			{
				//Сбор идентификаторов PlainFields полей не описанных в маппинге, но требующихся для получения
				//Важно: также исключаются идентификаторы ExtensionField, т.к. в qp они также представлены как Plain, но обработаны должны быть иначе
				List<int> plainFieldsDefIds = contentDef.Fields.Where(x => x is PlainField || x is ExtensionField).Select(x => x.FieldId).ToList();
				List<int> plainFieldsNotDefIds = article.FieldValues.Where(x => x.Field.RelationType == Qp8Bll.RelationType.None
																				&& !plainFieldsDefIds.Contains(x.Field.Id))
																	.Select(x => x.Field.Id).ToList(); //Список идентификаторов полей который не описаны в xml, но должны быть получены по LoadAllPlainFields="True"  
				if (plainFieldsNotDefIds.Count > 0) //Есть Plain поля не описанные в маппинге, но требуемые по аттрибуту LoadAllPlainFields="True"  
				{
					foreach (var fieldId in plainFieldsNotDefIds)
					{
						bool hasVirtualFields;

						var articleField = GetArticleField(fieldId, article, null, loadedArticlesCache, isLive, out hasVirtualFields, counter);

						if (articleField != null)
						{
							res.Fields.Add(articleField.FieldName, articleField);

							res.HasVirtualFields = res.HasVirtualFields || hasVirtualFields;
						}

					}
				}
			}

			//Заполнение полей из xaml-маппинга
			foreach (var fieldDef in contentDef.Fields.Where(x => !(x is Dictionaries) && !(x is BaseVirtualField)))
			{
				bool hasVirtualFields;

				var articleField = GetArticleField(fieldDef.FieldId, article, contentDef, loadedArticlesCache, isLive, out hasVirtualFields, counter, fieldDef.FieldName);

				if (articleField != null)
				{
					res.Fields.Add(articleField.FieldName, articleField);

					res.HasVirtualFields = res.HasVirtualFields || hasVirtualFields;
				}
			}

			return res;
		}

		/// <summary>
		/// Получение объекта поля (значение или связь с другим контентом)
		/// </summary>
		/// <param name="id">Идентификатор поля</param>
		/// <param name="article">Статья в которой читается поле</param>
		/// <param name="contentDef">Структура маппинга</param>
		/// <param name="hasVirtualFields"></param>
		/// <param name="fieldName">Опционально. Если передан, задает в результирующем объекте свойство FieldName</param>
		/// <param name="loadedArticles">кеш загруженных статей</param>
		/// <returns></returns>
		private ArticleField GetArticleField(int id, Qp8Bll.Article article, Content contentDef, Dictionary<ArticleShapedByDefinitionKey, Article> loadedArticles, bool isLive, out bool hasVirtualFields, ArticleCounter counter, string fieldName = "")
		{
			ArticleField res = null;

			if (article == null)
				throw new Exception(string.Format(ProductLoaderResources.ERR_XML_ARTICLE_EMPTY, id));

			Field fieldDef = null;

			if (contentDef != null)
			{
				var fieldsFromDef = contentDef.Fields.Where(x => x.FieldId == id).ToArray();

				if (fieldsFromDef.Length > 1)
					throw new Exception(string.Format("В Content с id={0} есть более одного поля с id={1}", contentDef.ContentId, id));

				if (fieldsFromDef.Length == 1)
					fieldDef = fieldsFromDef[0];
			}


			Qp8Bll.FieldValue field = null;

			if (!(fieldDef is BackwardRelationField)) //Поле может быть определено для любого, кроме обратного (т.к. при обратном подразумевается, что прямого нет)
			{
				field = article.FieldValues.SingleOrDefault(x => x.Field.Id == id);

				if (field == null)
					throw new Exception(string.Format(ProductLoaderResources.ERR_XML_FIELD_NOT_EXISTS, id, article.Id, article.ContentId));
			}

			hasVirtualFields = false;

			if (fieldDef == null || fieldDef is PlainField) //Для поля не переданы настройки маппинга или оно PlainField => если оно простое, надо получить его значение, если не простое -- пропускается
				res = new PlainArticleField
				{
					Value = field.Value,
					NativeValue = (field.Field.IsInteger || field.Field.RelationType == Qp8Bll.RelationType.OneToMany) && field.ObjectValue != null ? Convert.ToInt32(field.ObjectValue) : field.ObjectValue,
					PlainFieldType = (PlainFieldType)field.Field.ExactType /*map to our types*/
				};
			else if (fieldDef is BackwardRelationField)
			{
				var subContentDef = ((BackwardRelationField)fieldDef).Content;

				if (subContentDef == null)
					throw new Exception(string.Format(ProductLoaderResources.ERR_XML_FIELD_MAP_NOT_EXISTS, 0, "BackwardRelationField", id));

				var qpField = _fieldService.Read(id);

				int[] relatedArticlesIds = GetBackwardArticlesIds(qpField, article.Id);

				var items = new Dictionary<int, Article>();

				var articleToReadFromQpIds = new List<int>();

				foreach (int relatedArticleId in relatedArticlesIds)
				{
					string key = GetArticleKeyStringForCache(new ArticleShapedByDefinitionKey(relatedArticleId, subContentDef, isLive));

					var cachedArticle = _cacheProvider.Get(key, GetTags(subContentDef)) as Article;

					if (cachedArticle != null)
						items.Add(relatedArticleId, cachedArticle);
					else if (loadedArticles.TryGetValue(new ArticleShapedByDefinitionKey(relatedArticleId, subContentDef, isLive), out cachedArticle))
						items.Add(relatedArticleId, cachedArticle);
					else
						articleToReadFromQpIds.Add(relatedArticleId);
				}

				if (articleToReadFromQpIds.Count > 0)
				{
					var relatedQpArticles = _articleService.List(default(int), articleToReadFromQpIds.ToArray());

					foreach (var relatedQpArticle in relatedQpArticles)
					{
						var relatedArticle = GetArticle(relatedQpArticle, subContentDef, loadedArticles, isLive, counter);

						if (relatedArticle != null)
							items.Add(relatedArticle.Id, relatedArticle);
					}
				}

				res = new BackwardArticleField()
				{
					Items = items,
					FieldDisplayName = ((BackwardRelationField)fieldDef).DisplayName,
					ContentId = subContentDef.ContentId,
					SubContentId = subContentDef.ContentId,
					FieldId = id,
					FieldName = string.IsNullOrEmpty(fieldName) ? Guid.NewGuid().ToString() : fieldName
				};

				hasVirtualFields = items.Any(x => x.Value.HasVirtualFields);
			}
			else if (fieldDef is EntityField)   //Сложное поле (контент)
			{
				var subContentDef = ((EntityField)fieldDef).Content;

				if (subContentDef == null)
					throw new Exception(string.Format(ProductLoaderResources.ERR_XML_FIELD_MAP_NOT_EXISTS, field.Field.Id, field.Field.Name, id));

				var hasRelatedItems = field.RelatedItems.Count() > 0; //К данному полю-связи есть привязанные статьи

				if (field.Field.RelationType == Qp8Bll.RelationType.OneToMany)
				{
					Article item = null;

					if (hasRelatedItems)
					{
						int itemId = field.RelatedItems.First();

						item = GetArticle(itemId, subContentDef, loadedArticles, isLive, counter);
					}

					res = new SingleArticleField()
					{
						Item = item,
						Aggregated = field.Field.Aggregated,
						SubContentId = subContentDef.ContentId
					};

					if (item != null)
						hasVirtualFields = item.HasVirtualFields;
				}
				else if (field.Field.RelationType == Qp8Bll.RelationType.ManyToMany || field.Field.RelationType == Qp8Bll.RelationType.ManyToOne)
				{
					var items = new Dictionary<int, Article>();

					if (hasRelatedItems)
					{
						var articleToReadFromQpIds = new List<int>();

						foreach (var relatedArticleId in field.RelatedItems)
						{
							string key = GetArticleKeyStringForCache(new ArticleShapedByDefinitionKey(relatedArticleId, subContentDef, isLive));

							string[] tags = GetTags(subContentDef);

							var cachedArticle = _cacheProvider.Get(key, tags) as Article;

							if (cachedArticle != null)
								items.Add(relatedArticleId, cachedArticle);
							else if (loadedArticles.TryGetValue(new ArticleShapedByDefinitionKey(relatedArticleId, subContentDef, isLive), out cachedArticle))
								items.Add(relatedArticleId, cachedArticle);
							else
								articleToReadFromQpIds.Add(relatedArticleId);
						}

						if (articleToReadFromQpIds.Count > 0)
						{
							var relatedQpArticles = _articleService.List(default(int), articleToReadFromQpIds.ToArray());

							foreach (var relatedQpArticle in relatedQpArticles)
							{
								var relatedArticle = GetArticle(relatedQpArticle, subContentDef, loadedArticles, isLive, counter);

								if (relatedArticle != null)
									items.Add(relatedArticle.Id, relatedArticle);
							}
						}
					}
					res = new MultiArticleField()
					{
						Items = items,
						SubContentId = subContentDef.ContentId
					};

					hasVirtualFields = items.Any(x => x.Value.HasVirtualFields);
				}
			}
			else if (fieldDef is ExtensionField) //Поле-расширение
			{
				var contentMapping = ((ExtensionField)fieldDef).ContentMapping;

				if (contentMapping == null)
					throw new Exception(string.Format(ProductLoaderResources.ERR_XML_FIELD_EXT_MAP_NOT_EXISTS, id, article.Id));

				var extensionArticleField = new ExtensionArticleField();

				res = extensionArticleField;

				if (contentMapping.Count > 0 && article.AggregatedArticles != null && field.RelatedItems.Length > 0)
				{
					int extensionContentId = field.RelatedItems.First();

					Qp8Bll.Article extensionArticle = article.AggregatedArticles.FirstOrDefault(x => x.ContentId == extensionContentId);

					if (!contentMapping.ContainsKey(extensionContentId))
						throw new Exception(string.Format(ProductLoaderResources.ERR_XML_FIELD_MAP_NOT_EXISTS, field.Field.Id, field.Field.Name, id));

					Content subContentDef = contentMapping[extensionContentId];

					extensionArticleField.Value = field.Value;

					if (extensionArticle == null)
					{
						_logger.Error(
							string.Format("Статья {0} не имеет расширения, хотя поле {1} классификатора  запонено значением {2}.",
								article.Id, id, field.Value));
					}
					else
					{
						extensionArticleField.Item = GetArticle(extensionArticle, subContentDef, loadedArticles, isLive, counter);

						hasVirtualFields = extensionArticleField.Item.HasVirtualFields;
					}
				}
			}
			else
				throw new Exception(string.Format("Поле типа {0} не поддерживается", fieldDef.GetType()));

			if (field != null) //field == null только для BackwardArticleField
			{
				res.ContentId = field.Field.ContentId;
				res.FieldId = id;
				res.FieldName = string.IsNullOrEmpty(fieldName) ? field.Field.Name : fieldName;
				res.FieldDisplayName = field.Field.DisplayName;
			}

			res.CustomProperties = (fieldDef == null ? null : fieldDef.CustomProperties) ?? new Dictionary<string, object>();

			return res;
		}

		/// <summary>
		/// Получение идентификаторов статей которые связаны с данной по backward field
		/// </summary>
		/// <returns></returns>
		private int[] GetBackwardArticlesIds(Qp8Bll.Field field, int articleId)
		{
			string res = string.Empty;
			
			if (field.RelationType == Qp8Bll.RelationType.ManyToMany && field.LinkId.HasValue)
			{
				res = _articleService.GetLinkedItems(field.LinkId.Value, articleId);
			}
			else if (field.RelationType == Qp8Bll.RelationType.ManyToOne || field.RelationType == Qp8Bll.RelationType.OneToMany)
			{
				res = _articleService.GetRelatedItems(field.Id, articleId);
			}

			if (string.IsNullOrEmpty(res))
				return new int[] { };

			return res.Split(new char[] { ',' }).Select(x => int.Parse(x)).ToArray();
		}
		#endregion

		#region Инициализация: формирование словарей, зависимостей контентов


		private class ArticleShapedByDefinitionKey
		{
			public bool IsLive { get; set; }

			public Content Definition { get; set; }

			public int ArticleId { get; set; }

			public ArticleShapedByDefinitionKey(int articleId, Content definition, bool isLive)
			{
				ArticleId = articleId;

				Definition = definition;

				IsLive = isLive;
			}

			public override bool Equals(object obj)
			{
				var otherKey = obj as ArticleShapedByDefinitionKey;

				if (otherKey == null)
					return false;

				if (ReferenceEquals(this, otherKey))
					return true;

				return IsLive == otherKey.IsLive &&
					   ArticleId == otherKey.ArticleId &&
					   Definition.Equals(otherKey.Definition);
			}

			public override int GetHashCode()
			{
				return HashHelper.CombineHashCodes(HashHelper.CombineHashCodes(IsLive.GetHashCode(), Definition.GetHashCode()), ArticleId.GetHashCode());
			}
		}

		private Dictionary<ArticleShapedByDefinitionKey, Article> InitDictionaries(ProductDefinition productDefinition, bool isLive, ArticleCounter counter)
		{
			var dictionary = (Dictionaries)productDefinition.StorageSchema.Fields.SingleOrDefault(x => x is Dictionaries);

			var res = new Dictionary<ArticleShapedByDefinitionKey, Article>();

			if (dictionary == null || dictionary.ContentDictionaries.Count == 0)
				return res;

			const string keyCacheInitDic = "Dictionaries Cache for contentId={0}, isLive={1} hash={2}";

			var allContentsInDef = productDefinition.StorageSchema.GetChildContentsIncludingSelf();

			var allDicContentsSortedByLevel = allContentsInDef
												.Where(x => dictionary.ContentDictionaries.ContainsKey(x.ContentId))
												.OrderBy(x => x.GetChildContentsIncludingSelf().Length);

			foreach (var contentToCache in allDicContentsSortedByLevel)
			{
				TimeSpan cachePeriod = dictionary.GetCachePeriodForContent(contentToCache.ContentId).Value;

				var cacheTags = GetTags(contentToCache);

				var itemsToAdd = _cacheProvider.GetOrAdd(string.Format(keyCacheInitDic, contentToCache.ContentId, isLive, contentToCache.GetHashCode()), cacheTags, cachePeriod, () =>
				{
					var qpArticles = _articleService.List(contentToCache.ContentId, null);

					var contentDic = new Dictionary<ArticleShapedByDefinitionKey, Article>();

					foreach (var qpArticle in qpArticles)
					{
						var article = GetArticle(qpArticle, contentToCache, res, isLive, counter);

						if (article != null)
						{
							var dicKey = new ArticleShapedByDefinitionKey(article.Id, contentToCache, isLive);

							contentDic[dicKey] = article;
						}
					}

					return contentDic;
				});

				foreach (var itemFromCache in itemsToAdd)
					res[itemFromCache.Key] = itemFromCache.Value;
			}

			return res;
		}


		#endregion

		#region Кэширование
		private static string[] GetTags(Content content)
		{
			return DPCContentInvalidator.GetTagNameByContentId(content.GetChildContentsIncludingSelf().Select(x => x.ContentId));
		}

		private static string GetArticleKeyStringForCache(ArticleShapedByDefinitionKey articleKey)
		{
			return string.Join("_", KEY_CACHE_GET_ARTICLE, articleKey.ArticleId, articleKey.IsLive, articleKey.Definition.GetHashCode());
		}
        #endregion

        #region Articles limitation
        private int GetArticlesLimit()
        {
            int limit;

            string limitText = _settingsService.GetSetting(SettingsTitles.PRODUCT_ARTICLES_LIMIT);

            if (int.TryParse(limitText, out limit))
            {
                return limit;
            }
            else
            {
                return 0;
            }
        }

        private ArticleCounter GetArticleCounter(int productId)
        {
            return new ArticleCounter(GetArticlesLimit(), _logger, productId);
        }

        private ArticleCounter GetDictionaryCounter()
        {
            return new ArticleCounter(0, _logger, 0);
        }
        #endregion

        private Qp8Bll.Article ReadArticle(int id, bool isLive, Content contentDef = null)
		{
            var article = contentDef == null ? _articleService.Read(id) : _articleService.Read(id, contentDef.ContentId);

            if (article == null)
            {
                return null;
            }

            if (contentDef != null && article != null && article.ContentId != contentDef.ContentId)
			{
				throw new Exception(string.Format(ProductLoaderResources.ERR_XML_CONTENTID_DOES_NOT_MATCH_EXPECTED,
					article.ContentId, id, contentDef.ContentId));
			}
                  

			if (isLive && article.Status.Name != ARTICLE_STATUS_PUBLISHED)
				return null;

			return article;
		}
		

		#endregion	
	}

    internal class ArticleCounter
    {
        #region Private properties
        private readonly ILogger _logger;
        private readonly int _articlesLimit;
        private readonly int _productId;
        private int _totalCount;
        private int _cacheCount;
        private int _hitCount;
        //private Dictionary<int, int> _map;
        #endregion

        public ArticleCounter(int articlesLimit, ILogger logger, int productId)
        {
            _logger = logger;
            _articlesLimit = articlesLimit;
            _productId = productId;
            _totalCount = 0;
            _hitCount = 0;
            _cacheCount = 0;
            //_map = new Dictionary<int, int>();
        }

        #region Public methods
        public void CheckHitArticlesLimit(int id)
        {
            if (IsActive())
            {
                _hitCount++;
                CheckArticlesLimit(id);
            }
        }

        public void CheckCacheArticlesLimit(int id)
        {
            if (IsActive())
            {
                _cacheCount++;
                CheckArticlesLimit(id);
            }
        }

        public void LogCounter()
        {
            if (IsActive())
            {
                //_logger.Info($"CounterData ProductId: {_productId} TotalCount: {_totalCount} CacheCount: {_cacheCount} HitCount: {_hitCount} DistinctCount: {_map.Count}");
                _logger.Debug($"CounterData ProductId: {_productId} TotalCount: {_totalCount} CacheCount: {_cacheCount} HitCount: {_hitCount}");
            }
        }
        #endregion

        #region Private methods
        private void CheckArticlesLimit(int id)
        {
            _totalCount++;

            /*
            if (!_map.ContainsKey(id))
            {
                _map.Add(id, 0);
            }*/

            if (IsExceeded())
            {
                LogCounter();
                throw new Exception($"Продукт слишком большой и не подлежит обработке. Превышено ограничение в {_articlesLimit} статей.");
            }
        }

        private bool IsExceeded()
        {
            return _totalCount > _articlesLimit;
        }

        private bool IsActive()
        {
            return _articlesLimit > 0;
        }
        #endregion
    }
}
