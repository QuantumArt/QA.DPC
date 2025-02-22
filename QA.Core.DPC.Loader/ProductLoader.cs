﻿using QA.Core.Cache;
using QA.Core.DPC.Loader.Resources;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.QP.Cache;
using QA.Core.DPC.QP.Services;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.Core.Models.Filters;
using QA.Core.Models.Processors;
using QA.Core.Models.UI;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;
using QP8.Infrastructure.Extensions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Info;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NLog;
using NLog.Fluent;
using QA.Core.DPC.QP.Models;
using Content = QA.Core.Models.Configuration.Content;
using Qp8Bll = Quantumart.QP8.BLL;
using ConfigurationService = QP.ConfigurationService;

namespace QA.Core.DPC.Loader
{
    public class ProductLoader : IProductService
    {
        // ReSharper disable once InconsistentNaming
        private const string ARTICLE_STATUS_PUBLISHED = "Published";
        // ReSharper disable once InconsistentNaming
        private const string KEY_CACHE_GET_ARTICLE = "GetArticle_";

        private int _hits;
        private int _misses;

        private readonly Customer _customer;
        private readonly IContentDefinitionService _definitionService;
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly VersionedCacheProviderBase _cacheProvider;
        private readonly ICacheItemWatcher _cacheItemWatcher;
        private readonly IFieldService _fieldService;
        private readonly ISettingsService _settingsService;
        private readonly IList<IConsumerMonitoringService> _consumerMonitoringServices;
        private readonly IArticleFormatter _formatter;

        private IReadOnlyArticleService ArticleService { get; }

        #region Конструкторы
        public ProductLoader(IContentDefinitionService definitionService,
            VersionedCacheProviderBase cacheProvider, ICacheItemWatcher cacheItemWatcher,
            IReadOnlyArticleService articleService, IFieldService fieldService, ISettingsService settingsService,
            IList<IConsumerMonitoringService> consumerMonitoringServices, IArticleFormatter formatter, IConnectionProvider connectionProvider)
        {
            _definitionService = definitionService;
            _cacheProvider = cacheProvider;
            _cacheItemWatcher = cacheItemWatcher;
            ArticleService = articleService;
            _fieldService = fieldService;
            _settingsService = settingsService;
            _consumerMonitoringServices = consumerMonitoringServices;
            _formatter = formatter;

            _customer = connectionProvider.GetCustomer();
        }
        #endregion

        #region IProductService

        public virtual Dictionary<string, object>[] GetProductsList(ServiceDefinition definition, long startRow, long pageSize, bool isLive)
        {
            using (new Qp8Bll.QPConnectionScope(_customer.ConnectionString, (DatabaseType)_customer.DatabaseType))
            {
                var fields = _fieldService.List(definition.Content.ContentId).ToArray();

                var fieldNames = definition.Content.Fields
                    .Where(x => x is PlainField field && field.ShowInList)
                    .Select(x => fields.Single(y => y.Id == x.FieldId).Name)
                    .ToList();

                fieldNames.Add("CONTENT_ITEM_ID");

                var definitionArticles =
                    _customer.DbConnector.GetContentData(
                    new ContentDataQueryObject(_customer.DbConnector,
                        definition.Content.ContentId,
                        string.Join(",", fieldNames),
                        definition.Filter,
                        null,
                        startRow,
                        pageSize)
                    { ShowSplittedArticle = (byte)(isLive ? 0 : 1) });

                return
                    definitionArticles
                        .AsEnumerable()
                        .Select(x => fieldNames
                                    .Where(y => x[y] != DBNull.Value)
                                    .ToDictionary(y => y == "CONTENT_ITEM_ID" ? "id" : y, y => y == "CONTENT_ITEM_ID" || fields.Single(z => z.Name == y).IsInteger ? (int)(decimal)x[y] : x[y]))
                        .ToArray();
            }
        }

        /// <summary>
        /// Загрузить список статей <paramref name="articleIds"/> по описанию продукта <paramref name="content"/>
        /// </summary>
        public virtual Article[] GetProductsByIds(Content content, int[] articleIds, bool isLive = false)
        {
            using (new Qp8Bll.QPConnectionScope(_customer.ConnectionString, (DatabaseType)_customer.DatabaseType))
            {
                ArticleService.IsLive = isLive;

                _cacheItemWatcher.TrackChanges();

                var timer = new Stopwatch();
                timer.Start();

                ArticleService.LoadStructureCache();

                timer.Stop();
                _logger.ForTraceEvent()
                    .Message("LoadStructureCache for products {ids} took {elapsed} ms", 
                        articleIds, timer.Elapsed.Milliseconds)
                    .Log();
                
                var articlesById = new Dictionary<int, Article>();
                var missedArticleIds = new List<int>();

                foreach (int id in articleIds)
                {
                    var keyInCache = GetArticleKeyStringForCache(new ArticleShapedByDefinitionKey(id, content, isLive));
                    var article = (Article)_cacheProvider.Get(keyInCache);

                    if (article != null)
                    {
                        articlesById[id] = article;
                    }
                    else
                    {
                        articlesById[id] = null;
                        missedArticleIds.Add(id);
                    }
                }

                if (missedArticleIds.Any())
                {
                    _hits = _misses = 0;
                    timer.Restart();

                    var productDefinition = new ProductDefinition { StorageSchema = content };
                    var loadedArticles = InitDictionaries(productDefinition, isLive, GetDictionaryCounter());

                    timer.Stop();
                    _logger.ForInfoEvent()
                        .Message("InitDictionaries for products {ids} called with hits {hits} and misses {misses} took {elapsed} ms",
                            articleIds, _hits, _misses, timer.Elapsed.Milliseconds)
                        .Log();

                    timer.Restart();

                    var qpArticles = ReadArticles(missedArticleIds.ToArray(), isLive)
                        .Where(qpArticle => qpArticle != null)
                        .ToArray();

                    timer.Stop();
                    _logger.ForInfoEvent()
                        .Message("Root articles {ids} loading took {elapsed} ms",
                            missedArticleIds,  timer.Elapsed.Milliseconds)
                        .Log();

                    _hits = _misses = 0;
                    var counter = GetArticleCounter(content.ContentId);
                    timer.Restart();
                    
                    Article[] articles = GetArticlesForQpArticles(qpArticles, productDefinition.StorageSchema, loadedArticles, isLive, counter);

                    counter.LogCounter();
                    timer.Stop();
                    
                    _logger.ForInfoEvent()
                        .Message("GetArticlesForQpArticles for articles {ids} called with hits {hits} and misses {misses} took {elapsed} ms",
                            missedArticleIds, _hits, _misses, timer.Elapsed.Milliseconds)
                        .Log();

                    timer.Restart();

                    foreach (Article article in articles)
                    {
                        if (article.HasVirtualFields)
                        {
                            articlesById[article.Id] = GenerateArticleWithVirtualFields(article, productDefinition.StorageSchema);
                        }
                        else
                        {
                            articlesById[article.Id] = article;
                        }
                    }

                    timer.Stop();
                    
                    _logger.ForInfoEvent()
                        .Message("Virtual fields generating for products {ids} took {elapsed} ms",
                            missedArticleIds, timer.Elapsed.Milliseconds)
                        .Log();

                }

                return articleIds.Select(id => articlesById[id]).ToArray();
            }
        }

        /// <summary>
        /// Получение структурированного продукта на основе XML с маппингом данных
        /// </summary>
        public virtual Article GetProductById(int id, bool isLive = false, ProductDefinition productDefinition = null)
        {
            using (new Qp8Bll.QPConnectionScope(_customer.ConnectionString, (DatabaseType)_customer.DatabaseType))
            {
                ArticleService.IsLive = isLive;

                _cacheItemWatcher.TrackChanges();

                var timer = new Stopwatch();

                timer.Start();

                ArticleService.LoadStructureCache();
                
                timer.Stop();
                _logger.ForTraceEvent()
                    .Message("LoadStructureCache for product {id} took {elapsed} ms", 
                        id, timer.Elapsed.Milliseconds)
                    .Log();

                Article article = null;

                if (productDefinition != null)
                {
                    var keyInCache = GetArticleKeyStringForCache(new ArticleShapedByDefinitionKey(id, productDefinition.StorageSchema, isLive));

                    article = (Article)_cacheProvider.Get(keyInCache);
                }

                if (article == null)
                {
                    timer.Reset();
                    timer.Start();

                    var counter = GetArticleCounter(id);

                    var qpArticle = ReadArticles(new[] { id }, isLive).FirstOrDefault();

                    if (qpArticle == null)
                        return null;

                    timer.Stop();
                    _logger.ForInfoEvent()
                        .Message("Root article {id} loading took {elapsed} ms",
                            id,  timer.Elapsed.Milliseconds)
                        .Log();

                    timer.Restart();

                    if (productDefinition == null)
                    {
                        Int32.TryParse(qpArticle.FieldValues.FirstOrDefault(x => x.Field.IsClassifier)?.Value, out var productTypeId);

                        productDefinition = GetProductDefinition(productTypeId, qpArticle.ContentId, isLive);
                    }

                    timer.Stop();
                    _logger.ForInfoEvent()
                        .Message("Product {id} definition loading took {elapsed} ms",
                            id,  timer.Elapsed.Milliseconds)
                        .Log();

                    timer.Restart();

                    article = GetProduct(qpArticle, productDefinition, isLive, counter);
                    counter.LogCounter();

                    timer.Stop();
                    _logger.ForInfoEvent()
                        .Message("Product {id} loading took {elapsed} ms",
                            id,  timer.Elapsed.Milliseconds)
                        .Log();
                }

                if (article.HasVirtualFields)
                {
                    timer.Restart();

                    article = GenerateArticleWithVirtualFields(article, productDefinition.StorageSchema);

                    timer.Stop();
                    _logger.ForInfoEvent()
                        .Message("Virtual fields generating for product {id} took {elapsed} ms",
                            id,  timer.Elapsed.Milliseconds)
                        .Log();
                }

                return article;
            }
        }
        
        public virtual Article[] GetProductsByIds(int[] ids, bool isLive = false)
        {
            var dbConnector = _customer.DbConnector;
            var idList = SqlQuerySyntaxHelper.IdList(_customer.DatabaseType, "@ids", "ids");

            var dbCommand = dbConnector.CreateDbCommand(
                $@"SELECT CONTENT_ID, CONTENT_ITEM_ID 
                    FROM CONTENT_ITEM {SqlQuerySyntaxHelper.WithNoLock(_customer.DatabaseType)} 
                    WHERE CONTENT_ITEM_ID IN (SELECT ID FROM {idList})");

            dbCommand.CommandType = CommandType.Text;
            dbCommand.Parameters.Add(SqlQuerySyntaxHelper.GetIdsDatatableParam("@ids", ids, _customer.DatabaseType));
            
            var dt = dbConnector.GetRealData(dbCommand);

            return dt
                .AsEnumerable()
                .GroupBy(x => (int)(decimal)x["CONTENT_ID"])
                .SelectMany(x => GetProductsByIds(x.Key, x.Select(y => (int)(decimal)y["CONTENT_ITEM_ID"]).ToArray(), isLive))
                .ToArray();
        }

        public virtual Article[] GetProductsByIds(int contentId, int[] ids, bool isLive = false)
        {
            var res = new List<Article>();
            using (new Qp8Bll.QPConnectionScope(_customer.ConnectionString, (DatabaseType)_customer.DatabaseType))
            {
                ArticleService.IsLive = isLive;
                _cacheItemWatcher.TrackChanges();
                ArticleService.LoadStructureCache();

                var articles = ArticleService.List(contentId, ids).ToArray();
                if (!articles.Any())
                {
                    return new Article[] { };
                }

                //Группируем статьи по значению productTypeId (идентификатору контента-расширения)
                var groupedArticles = from article in articles
                                      group article by article.FieldValues.Where(a => a.Field.IsClassifier).Select(a => a.Value).FirstOrDefault() into gr
                                      select gr;

                foreach (var group in groupedArticles)
                {
                    var productTypeId = int.Parse(group.Key ?? "0");

                    var productDefinition = GetProductDefinition(productTypeId, contentId, isLive);

                    // это кеш загруженных статей в рамках одного запроса (подходит для тупого кеширования в лоб словарей)
                    // по идее словари надо предзагружать и следить за их изменением
                    if (productDefinition == null)
                        throw new Exception(
                            $"Product definitions not found for articles: {string.Join(", ", ids)} ");

                    var loadedArticles = InitDictionaries(productDefinition, isLive, GetDictionaryCounter());

                    res.AddRange(
                        group
                            .Select(product =>
                            {
                                var counter = GetArticleCounter(product.Id);
                                var article = GetArticlesForQpArticles(new[]{product}, productDefinition.StorageSchema, loadedArticles, isLive, counter).FirstOrDefault();

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
            var missingSet = items
                .Where(item => string.IsNullOrEmpty(item.TypeAttributeName) && string.IsNullOrEmpty(item.ExtensionName))
                .Select(itm => itm.ProductId).ToHashSet();
            var existingItems = items.Where(itm => !missingSet.Contains(itm.ProductId)).ToArray();
            var missingIds = missingSet.ToArray();
            if (missingIds.Any())
            {
                foreach (var consumerMonitoringService in _consumerMonitoringServices)
                {
                    var idsToСlarify = consumerMonitoringService.FindExistingProducts(missingIds);
                    products.AddRange(idsToСlarify.Select(id => GetReferenceProduct(consumerMonitoringService, id)));
                    missingIds = missingIds.Except(idsToСlarify).ToArray();

                    if (!missingIds.Any())
                    {
                        break;
                    }
                }

                products.AddRange(missingIds.Select(GetMissingProduct));
            }

            products.AddRange(existingItems.Select(GetExistingProduct));

            return products.ToArray();

        }

        private static Article GetExistingProduct(ProductInfo item)
        {
            var product = new Article
            {
                Id = item.ProductId,
                Visible = true,
                Archived = false,
                IsPublished = true,
                ContentId = item.ContentId,
                ContentName = item.ContentName,
                ContentDisplayName = item.ContentDisplayName
            };

            if (!string.IsNullOrEmpty(item.TypeAttributeName) && item.TypeAttributeName != item.ExtensionAttributeName)
            {
                product.Fields.Add(item.TypeAttributeName, new PlainArticleField
                {
                    ContentId = item.ContentId,
                    FieldId = 0,
                    FieldName = item.TypeAttributeName,
                    Value = item.TypeAttributeValue,
                    NativeValue = item.TypeAttributeValue,
                    PlainFieldType = PlainFieldType.String
                });
            }

            if (!string.IsNullOrEmpty(item.ExtensionAttributeName))
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

        private Article GetMissingProduct(int id)
        {
            return new Article
            {
                Id = id,
                ContentId = 0,
                Visible = true,
                Archived = false,
                ContentName = string.Empty,
                IsPublished = true
            };
        }

        private Article GetReferenceProduct(IConsumerMonitoringService consumerMonitoringService, int id)
        {
            var productData = consumerMonitoringService.GetProduct(id);

            if (!string.IsNullOrEmpty(productData))
            {
                using (var prductStream = new MemoryStream(Encoding.UTF8.GetBytes(productData)))
                {
                    var procTask = _formatter.Read(prductStream);
                    procTask.Wait();
                    return procTask.Result;
                }
            }

            return GetMissingProduct(id);
        }

        /// <summary>
        /// Получение структуры данных на основе XML с мапингом данных из БД
        /// </summary>
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
            var res = new ProductDefinition
            {
                ProdictTypeId = productTypeId,
                StorageSchema = _definitionService.TryGetDefinitionForContent(productTypeId, contentId, isLive)
            };


            if (res.StorageSchema == null)
                throw new Exception(
                    $"Definition not found: productTypeId={productTypeId}, contentId={contentId}, isLive={isLive}");

            return res;
        }
        #endregion

        #region Закрытые методы

        #region Queries

        private string GetProductTypesQuery(string idsExpression)
        { 
                return $@"SELECT
            ids.Id AS ProductId,
                c.CONTENT_ID AS ContentId,
                c.NET_CONTENT_NAME AS ContentName,
                c.CONTENT_NAME AS ContentDisplayName,
                itm.VISIBLE AS Visible,
                itm.ARCHIVE AS Archive,
                cl.ATTRIBUTE_NAME AS ExtensionAttributeName,
                ec.CONTENT_ID AS ExtensionContentId,
                ec.NET_CONTENT_NAME AS ExtensionName,
                ec.CONTENT_NAME AS ExtensionDisplayName,
                ta.ATTRIBUTE_NAME AS TypeAttributeName,
                tv.DATA AS TypeAttributeValue
            FROM {idsExpression}
            LEFT JOIN CONTENT_ITEM itm ON ids.Id = itm.CONTENT_ITEM_ID
            LEFT JOIN Content c ON itm.CONTENT_ID = c.CONTENT_ID
            LEFT JOIN(select content_id, attribute_id, attribute_name from
            CONTENT_ATTRIBUTE where is_classifier = {SqlQuerySyntaxHelper.ToBoolSql(_customer.DatabaseType, true)}) cl on c.CONTENT_ID = cl.CONTENT_ID
            LEFT JOIN CONTENT_DATA cd on cd.CONTENT_ITEM_ID = itm.CONTENT_ITEM_ID and cd.ATTRIBUTE_ID = cl.ATTRIBUTE_ID
            LEFT JOIN content ec on cd.DATA = CAST(ec.CONTENT_ID as varchar)
            LEFT JOIN CONTENT_ATTRIBUTE ta ON itm.CONTENT_ID = ta.CONTENT_ID AND ta.ATTRIBUTE_NAME = @typeField
            LEFT JOIN CONTENT_DATA
                tv ON ta.ATTRIBUTE_ID = tv.ATTRIBUTE_ID AND itm.CONTENT_ITEM_ID = tv.CONTENT_ITEM_ID";
        }

        public ProductInfo[] GetProductsInfo(int[] productIds)
        {
            var typeField = _settingsService.GetSetting(SettingsTitles.PRODUCT_TYPES_FIELD_NAME);

            using (var cs = new Qp8Bll.QPConnectionScope(_customer.ConnectionString, (DatabaseType)_customer.DatabaseType))
            {
                var cnn = new DBConnector(cs.DbConnection);
                var idList = SqlQuerySyntaxHelper.IdList(_customer.DatabaseType, "@ids", "ids");
                using (var cmd = cnn.CreateDbCommand(GetProductTypesQuery(idList)))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(SqlQuerySyntaxHelper.GetIdsDatatableParam("@ids", productIds, _customer.DatabaseType));
                    cmd.Parameters.AddWithValue("@typeField", typeField);
                    var data = cnn.GetRealData(cmd);
                    return data.AsEnumerable().Select(Converter.ToModelFromDataRow<ProductInfo>).ToArray();
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
            public string TypeAttributeName { get; set; }
            public string TypeAttributeValue { get; set; }
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

            _logger.ForInfoEvent()
                .Message("InitDictionaries called with hits {hits} and misses {misses} took {elapsed} ms",
                    _hits, _misses, stopWatch.Elapsed.Milliseconds)
                .Log();

            _hits = _misses = 0;
            stopWatch.Restart();

            var article = GetArticlesForQpArticles(new []{qpArticle}, productDefinition.StorageSchema, loadedArticles, isLive, counter).FirstOrDefault();

            stopWatch.Stop();
            _logger.ForInfoEvent()
                .Message("GetArticle called with hits {hits} and misses {misses} took {elapsed} ms",
                    _hits, _misses, stopWatch.Elapsed.Milliseconds)          
                .Log();

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

            foreach (var articleField in article.Fields.Values)
            {
                if (articleField is SingleArticleField singleArticleField)
                {
                    if (singleArticleField.Item != null)
                    {
                        var field = content.Fields.Single(x => x.FieldId == articleField.FieldId);

                        var childContent = field is EntityField entityField
                            ? entityField.Content
                            : ((ExtensionField)field).ContentMapping[singleArticleField.Item.ContentId];

                        FillVirtualFields(singleArticleField.Item, childContent, processedArticleKeys);
                    }
                }
                else if (articleField is MultiArticleField multiArticleField)
                {
                    var field = content.Fields.Single(x => x.FieldId == articleField.FieldId);

                    var childContent = ((EntityField)field).Content;

                    foreach (var childArticle in multiArticleField.Items.Values)
                        FillVirtualFields(childArticle, childContent, processedArticleKeys);
                }
            }

            foreach (var virtualField in content.Fields.Where(x => x is BaseVirtualField).Cast<BaseVirtualField>())
            {
                var field = CreateAnyTypeVirtualField(virtualField, article);
                if (field != null)
                {
                    article.Fields.Add(field.FieldName, field);
                }
            }
        }

        private ArticleField CreateAnyTypeVirtualField(BaseVirtualField virtualField, Article article)
        {
            ArticleField field;
            if (virtualField is VirtualField field1)
            {
                field = CreateVirtualField(field1, article);
            }
            else if (virtualField is VirtualMultiEntityField entityField)
            {
                field = CreateVirtualMiltiEntityField(entityField, article);
            }
            else if (virtualField is VirtualEntityField virtualEntityField)
            {
                field = CreateVirtualArticleField(virtualEntityField, article);
            }
            else
            {
                throw new Exception($"Virtual field type {virtualField.GetType()} is not supported");
            }

            return field;
        }

        private ArticleField CreateVirtualMiltiEntityField(VirtualMultiEntityField virtualMultiEntityField, Article article)
        {
            var articlesWithParents = DPathProcessor.Process(virtualMultiEntityField.Path, article);
            if (!articlesWithParents.Any())
            {
                return null;
            }

            if (articlesWithParents.Any(x => !(x.ModelObject is Article)))
            {
                throw new Exception("VirtualMultiEntityField requires article filter in Path");
            }

            foreach (var articleWithParent in articlesWithParents)
            {
                RemoveModelObject(articleWithParent);
            }

            return new VirtualMultiArticleField
            {
                VirtualArticles = articlesWithParents.Select(x => CreateVirtualArticleField(virtualMultiEntityField, (Article)x.ModelObject)).Where(x => x != null).ToArray(),
                FieldName = virtualMultiEntityField.FieldName
            };
        }

        private VirtualArticleField CreateVirtualArticleField(VirtualEntityField virtualEntityField, Article article)
        {
            var fields = virtualEntityField.Fields.Select(x => CreateAnyTypeVirtualField(x, article)).Where(x => x != null).ToArray();
            return fields.Any() ? new VirtualArticleField { FieldName = virtualEntityField.FieldName, Fields = fields } : null;
        }

        private static ArticleField CreateVirtualField(VirtualField virtualField, Article article)
        {
            var foundObjectWithParent = DPathProcessor.Process(virtualField.Path, article).FirstOrDefault();
            if (!virtualField.PreserveSource)
            {
                if (!string.IsNullOrEmpty(virtualField.ObjectToRemovePath))
                {
                    var objectToRemove = DPathProcessor.Process(virtualField.ObjectToRemovePath, article).FirstOrDefault();
                    if (objectToRemove != null)
                    {
                        RemoveModelObject(objectToRemove);
                    }
                }

                if (foundObjectWithParent != null)
                {
                    RemoveModelObject(foundObjectWithParent);
                }
            }

            var foundObject = foundObjectWithParent?.ModelObject;
            var covertedObject = virtualField.Converter == null ? foundObject : virtualField.Converter.Convert(foundObject);
            if (covertedObject == null)
            {
                return null;
            }

            if (covertedObject is ArticleField field)
            {
                field.FieldName = virtualField.FieldName;
                return field;
            }

            if (covertedObject is Article foundArticle)
            {
                return new SingleArticleField { Item = foundArticle, FieldName = virtualField.FieldName };
            }

            return new PlainArticleField
            {
                FieldName = virtualField.FieldName,
                NativeValue = covertedObject,
                Value = covertedObject.ToString()
            };
        }

        private static void RemoveModelObject(ModelObjectWithParent modelObjectWithParent)
        {
            if (modelObjectWithParent.ModelObject is ArticleField articleField)
            {
                ((Article)modelObjectWithParent.Parent).Fields.Remove(articleField.FieldName);
            }
            else
            {
                if (modelObjectWithParent.ModelObject is Article article)
                {
                    ((MultiArticleField)modelObjectWithParent.Parent).Items.Remove(article.Id);
                }
            }
        }
        #endregion

        #region Чтение на основе XML

        /// <summary>
        /// Получение статьи и всех ее полей, описанных в структуре маппинга
        /// </summary>
        private Article[] GetArticlesForQpArticles(Qp8Bll.Article[] qpArticles, Content contentDef,
            Dictionary<ArticleShapedByDefinitionKey, Article> localCache, bool isLive, ArticleCounter counter)
        {
            if (qpArticles == null || !qpArticles.Any())
            {
                return new Article[0];
            }
            if (contentDef == null)
            {
                string idStr = String.Join(", ", qpArticles.Select(n => n.Id.ToString()));
                throw new Exception(String.Format(ProductLoaderResources.ERR_XML_CONTENT_MAP_NOT_EXISTS, idStr));
            }

            int[] articleIds = qpArticles.Select(n => n.Id).ToArray();
            counter.CheckHitArticlesLimit(articleIds);

            qpArticles = qpArticles.Where(n => !isLive || n.Status.Name == ARTICLE_STATUS_PUBLISHED).ToArray();
            var result = new Dictionary<string, Article>();

            if (qpArticles.Any())
            {
                var localKeys = qpArticles.Select(a => new ArticleShapedByDefinitionKey(a.Id, contentDef, isLive));
                
                var localCacheMisses = new HashSet<ArticleShapedByDefinitionKey>();
                
                foreach (var localKey in localKeys)
                {
                    if (localCache.TryGetValue(localKey, out Article res))
                    {
                        _hits += 1;
                        counter.CheckCacheArticlesLimit(new[] { localKey.ArticleId });
                        result.Add(GetArticleKeyStringForCache(localKey), res);
                    }
                    else
                    {
                        _misses += 1;
                        localCacheMisses.Add(localKey);
                    }
                }
                
                var newArticlePairs = qpArticles.Select(a => new
                    {
                        LocalKey = new ArticleShapedByDefinitionKey(a.Id, contentDef, isLive),
                        QpArticle = a,
                    })
                    .Where(n => localCacheMisses.Contains(n.LocalKey)).Select(p => new
                    {
                        p.LocalKey,
                        p.QpArticle,
                        Article = new Article()
                        {
                            ContentId = contentDef.ContentId,
                            IsReadOnly = contentDef.IsReadOnly,
                            Archived = p.QpArticle.Archived,
                            ContentDisplayName = p.QpArticle.DisplayContentName,
                            PublishingMode = contentDef.PublishingMode,
                            ContentName = p.QpArticle.Content.NetName,
                            Created = p.QpArticle.Created,
                            Modified = p.QpArticle.Modified,
                            IsPublished = p.QpArticle.Status.Name == ARTICLE_STATUS_PUBLISHED && !p.QpArticle.Delayed,
                            Splitted = p.QpArticle.Splitted,
                            Status = p.QpArticle.Status.Name,
                            Visible = p.QpArticle.Visible,
                            Id = p.QpArticle.Id,
                            HasVirtualFields = contentDef.Fields.Any(x => x is BaseVirtualField)
                        }
                    })
                    .ToArray();
                
                //кладем в словарь Articles до а не после загрузки полей так как может быть цикл по данным одновременно с циклом по дефинишенам
                //и иначе бы был stackoverflow на вызовах GetArticlesNotCached->GetArticlesField->GetArticlesNotCached->...
                foreach (var pair in newArticlePairs)
                {
                    localCache[pair.LocalKey] = pair.Article;
                    var globalKey = GetArticleKeyStringForCache(pair.LocalKey);
                    result.Add(globalKey, pair.Article);
                }

                Qp8Bll.Article[] newQpArticles = newArticlePairs.Select(p => p.QpArticle).ToArray();

                if (newQpArticles.Any())
                {
                    //Заполнение Plain-полей по параметру LoadAllPlainFields="True"
                    if (contentDef.LoadAllPlainFields)
                    {
                        //Сбор идентификаторов PlainFields полей не описанных в маппинге, но требующихся для получения
                        //Важно: также исключаются идентификаторы ExtensionField, т.к. в qp они также представлены как Plain, но обработаны должны быть иначе
                        var plainFieldsDefIds = contentDef.Fields.Where(x => x is PlainField || x is ExtensionField)
                            .Select(x => x.FieldId).ToList();
                        var plainFieldsNotDefIds = qpArticles.First().FieldValues.Where(x =>
                                x.Field.RelationType == Qp8Bll.RelationType.None
                                && !plainFieldsDefIds.Contains(x.Field.Id))
                            .Select(x => x.Field.Id)
                            .ToList(); //Список идентификаторов полей который не описаны в xml, но должны быть получены по LoadAllPlainFields="True"
                        if (plainFieldsNotDefIds.Count > 0)
                        //Есть Plain поля не описанные в маппинге, но требуемые по аттрибуту LoadAllPlainFields="True"
                        {
                            foreach (var fieldId in plainFieldsNotDefIds)
                            {
                                var articleFields = GetArticleField(fieldId, newQpArticles, null, localCache, isLive, counter);
                                bool hasVirtualFields = CheckVirtualFields(articleFields);
                                foreach (var localKey in newArticlePairs.Select(a => a.LocalKey))
                                {
                                    var articleField = articleFields[localKey.ArticleId];
                                    var currentRes = localCache[localKey];
                                    if (articleField != null)
                                    {
                                        currentRes.Fields.Add(articleField.FieldName, articleField);
                                        currentRes.HasVirtualFields = currentRes.HasVirtualFields || hasVirtualFields;
                                    }
                                }
                            }
                        }
                    }
                
                    //Заполнение полей из xaml-маппинга
                    foreach (var fieldDef in contentDef.Fields.Where(x => !(x is Dictionaries) && !(x is BaseVirtualField)))
                    {
                        var articleFields = GetArticleField(
                            fieldDef.FieldId, newQpArticles, contentDef, localCache,
                            isLive, counter, fieldDef.FieldName);

                        var hasVirtualFields = CheckVirtualFields(articleFields);
                
                        foreach (var localKey in newArticlePairs.Select(a => a.LocalKey))
                        {
                            if (articleFields.TryGetValue(localKey.ArticleId, out var articleField))
                            {
                                var currentRes = localCache[localKey];
                                if (articleField != null)
                                {
                                    currentRes.Fields.Add(articleField.FieldName, articleField);
                                    currentRes.HasVirtualFields = currentRes.HasVirtualFields || hasVirtualFields;
                                }
                            }
                        }
                    }
                }
            }

            return result.Select(n => n.Value).ToArray();
        }

        /// <summary>
        /// Получение объекта поля (значение или связь с другим контентом)
        /// </summary>
        private Dictionary<int, ArticleField> GetArticleField(int fieldId, Qp8Bll.Article[] articles, Content contentDef, Dictionary<ArticleShapedByDefinitionKey, Article> localCache, bool isLive, ArticleCounter counter, string fieldName = "")
        {
            var result = new Dictionary<int, ArticleField>();

            if (articles == null || !articles.Any())
                throw new Exception(string.Format(ProductLoaderResources.ERR_XML_ARTICLE_EMPTY, fieldId));

            Field fieldDef = null;

            if (contentDef != null)
            {
                var fieldsFromDef = contentDef.Fields.Where(x => x.FieldId == fieldId).ToArray();

                if (fieldsFromDef.Length > 1)
                    throw new Exception($"В Content с id={contentDef.ContentId} есть более одного поля с id={fieldId}");

                if (fieldsFromDef.Length == 1)
                    fieldDef = fieldsFromDef[0];
            }


            Qp8Bll.FieldValue[] fieldValues = articles.Select(n => n.FieldValues.SingleOrDefault(m => m.Field.Id == fieldId)).ToArray();
            var fieldValue = fieldValues.First();
            var articleIds = articles.Select(n => n.Id).ToArray();

            if (!(fieldDef is BackwardRelationField)) //Поле может быть определено для любого, кроме обратного (т.к. при обратном подразумевается, что прямого нет)
            {
                if (!fieldValues.Any() || fieldValues.First() == null)
                {
                    var article = articles.First();
                    throw new Exception(string.Format(ProductLoaderResources.ERR_XML_FIELD_NOT_EXISTS, fieldId, article.Id, article.ContentId));
                }

            }
            if (fieldDef == null || fieldDef is PlainField) //Для поля не переданы настройки маппинга или оно PlainField => если оно простое, надо получить его значение, если не простое -- пропускается
            {
                result.AddRange(ProcessSimpleFields(fieldValues));
            }
            else if (fieldDef is BackwardRelationField)
            {
                result.AddRange(ProcessBackwardRelationField(fieldId, articleIds, localCache, isLive, counter, fieldName, fieldDef));
            }
            else if (fieldDef is EntityField field)   //Сложное поле (контент)
            {
                var subContentDef = field.Content;

                if (subContentDef == null)
                    throw new Exception(string.Format(ProductLoaderResources.ERR_XML_FIELD_MAP_NOT_EXISTS, fieldValue.Field.Id, fieldValue.Field.Name, fieldId));

                if (fieldValue.Field.RelationType == Qp8Bll.RelationType.OneToMany)
                {
                    result.AddRange(ProcessOneToManyField(localCache, isLive, counter, fieldValues, subContentDef));
                }
                else if (fieldValue.Field.RelationType == Qp8Bll.RelationType.ManyToMany || fieldValue.Field.RelationType == Qp8Bll.RelationType.ManyToOne)
                {
                    result.AddRange(ProcessManyField(localCache, isLive, counter, fieldValues, subContentDef));
                }
            }
            else if (fieldDef is ExtensionField) //Поле-расширение
            {
                result.AddRange(ProcessExtensionField(fieldId, articles, localCache, isLive, counter, fieldDef));
            }
            else
                throw new Exception($"Field type {fieldDef.GetType()} is not supported");

            AppendCommonProperties(result, fieldValue, fieldDef, fieldName);

            return result;
        }

        private IEnumerable<KeyValuePair<int, ArticleField>> ProcessExtensionField(int fieldId, Qp8Bll.Article[] articles, Dictionary<ArticleShapedByDefinitionKey, Article> localCache, bool isLive,
            ArticleCounter counter, Field fieldDef)
        {            
            var contentMapping = ((ExtensionField)fieldDef).ContentMapping;
            var contextMap = new ConcurrentDictionary<int, List<ExtensionContext>>();

            foreach (var article in articles)
            {
                var fieldValue = article.FieldValues.FirstOrDefault(fv => fv.Field.Id == fieldId);                

                if (fieldValue != null && int.TryParse(fieldValue.Value, out int exstensionContentId))
                {
                    if (contentMapping.TryGetValue(exstensionContentId, out Content exstensionDef))
                    {
                        var exstensionArticle = article.AggregatedArticles.FirstOrDefault(a => a.ContentId == exstensionContentId);

                        if (exstensionArticle != null)
                        {
                            var list = contextMap.GetOrAdd(exstensionContentId, key => new List<ExtensionContext>());

                            list.Add(new ExtensionContext
                            {
                                ArticleId = article.Id,
                                ExtensionId = fieldValue.Value,
                                ExtensionArticle = exstensionArticle
                            });                       
                        }
                    }
                }
            }

            foreach(var kv in contextMap)
            {
                var extensions = kv.Value.Select(x => x.ExtensionArticle).ToArray();
                var exstensionDef = contentMapping[kv.Key];
                var extArticles = GetArticlesForQpArticles(extensions, exstensionDef, localCache, isLive, counter);

                foreach(var exstension in kv.Value)
                {
                    yield return new KeyValuePair<int, ArticleField>(
                        exstension.ArticleId,
                        new ExtensionArticleField
                        {
                            Value = exstension.ExtensionId,
                            Item = extArticles.FirstOrDefault(a => a.Id == exstension.ExtensionArticle.Id)
                        });
                }
            }            
        }

        private IEnumerable<KeyValuePair<int, ArticleField>> ProcessManyField(Dictionary<ArticleShapedByDefinitionKey, Article> localCache, bool isLive, ArticleCounter counter, Qp8Bll.FieldValue[] fieldValues,
            Content subContentDef)
        {
            var flattenedRelatedArticlesIds = fieldValues.SelectMany(n => n.RelatedItems).Distinct().ToArray();

            var articleBag = GetArticleBag(flattenedRelatedArticlesIds, subContentDef, localCache, isLive, counter);

            foreach (var fieldValue in fieldValues)
            {
                var relatedItems = fieldValue.RelatedItems
                    .Select(n => articleBag.TryGetValue(n, out var article) ? article : null)
                    .Where(n => n != null);

                var dict = relatedItems.ToDictionary(relatedItem => relatedItem.Id);

                var res = new MultiArticleField
                {
                    Items = dict,
                    SubContentId = subContentDef.ContentId
                };

                var result = new KeyValuePair<int, ArticleField>(fieldValue.Article.Id, res);
                yield return result;

            }
        }

        private Dictionary<int, Article> GetArticleBag(int[] articleIds, Content subContentDef, Dictionary<ArticleShapedByDefinitionKey, Article> localCache, bool isLive, ArticleCounter counter)
        {
            var result = new Dictionary<int, Article>();

            if (articleIds == null)
                return result;

            if (subContentDef == null)
            {
                var idstr = String.Join(", ", articleIds.Select(n => n.ToString()));
                throw new Exception(string.Format(ProductLoaderResources.ERR_XML_CONTENT_MAP_NOT_EXISTS, idstr));
            }

            var cacheMisses = new List<string>();
            var keyItems = articleIds.Select(n => new ArticleShapedByDefinitionKey(n, subContentDef, isLive))
                .Select(n => new { LocalKey = n, GlobalKey =  GetArticleKeyStringForCache(n)}).ToArray();
            var tags = GetTags(subContentDef);

            foreach (var keyItem in keyItems)
            {
                var id = keyItem.LocalKey.ArticleId;
                if (_cacheProvider.Get(keyItem.GlobalKey, tags) is Article cachedArticle)
                    result.Add(id, cachedArticle);
                else if (localCache.TryGetValue(new ArticleShapedByDefinitionKey(id, subContentDef, isLive),
                    out cachedArticle))
                    result.Add(id, cachedArticle);
                else
                    cacheMisses.Add(keyItem.GlobalKey);
            }

            if (cacheMisses.Any())
            {

                var cachePeriod = subContentDef.GetCachePeriodForContent();


                if (cachePeriod != null)
                {
                    var cacheValues = _cacheProvider.GetOrAddValues(cacheMisses.ToArray(), string.Empty, tags, cachePeriod.Value, Func);
                    result.AddRange(cacheValues.ToDictionary(n => n.Id, m => m));
                }
                else
                {
                    var rawValues = Func(cacheMisses.ToArray());
                    result.AddRange(rawValues.Values.ToDictionary(n => n.Id, m => m));
                }

            }

            return result;

            
            Dictionary<string, Article> Func(string[] keysToLoad)
            {
                var keyPairs = keysToLoad.ToDictionary(ArticleShapedByDefinitionKey.Parse, m => m);
                var idsToLoad = keyPairs.Keys.ToArray();
                var qpArticles = ReadArticles(idsToLoad, isLive, subContentDef);
                return GetArticlesForQpArticles(qpArticles, subContentDef, localCache, isLive, counter)
                    .ToDictionary(n => keyPairs[n.Id], m => m);
            }

        }

        private bool CheckVirtualFields(Dictionary<int, ArticleField> dict)
        {
            var result = false;
            var field = dict.FirstOrDefault().Value;
            if (field is SingleArticleField sf)
            {
                result = sf.Item?.HasVirtualFields ?? false;
            }
            else if (field is MultiArticleField mf)
            {
                result = mf.Items.Any() && mf.Items.First().Value.HasVirtualFields;
            }
            return result;
        }

        private void AppendCommonProperties(Dictionary<int, ArticleField> dict, Qp8Bll.FieldValue fieldValue, Field fieldDef, string fieldName)
        {
            foreach (var elem in dict)
            {
                if (fieldValue != null) //field == null только для BackwardArticleField
                {
                    elem.Value.ContentId = fieldValue.Field.ContentId;
                    elem.Value.FieldId = fieldValue.Field.Id;
                    elem.Value.FieldName = string.IsNullOrEmpty(fieldName) ? fieldValue.Field.Name : fieldName;
                    elem.Value.FieldDisplayName = fieldValue.Field.DisplayName;
                }
                elem.Value.CustomProperties = fieldDef?.CustomProperties ?? new Dictionary<string, object>();               
            }

        }

        private IEnumerable<KeyValuePair<int, ArticleField>> ProcessOneToManyField(Dictionary<ArticleShapedByDefinitionKey, Article> localCache, bool isLive,
            ArticleCounter counter, Qp8Bll.FieldValue[] fieldValues, Content subContentDef)
        {
            var itemIds = fieldValues.Where(n => n.RelatedItems.Any()).Select(n => n.RelatedItems.First()).Distinct().ToArray();

            var articleBag = GetArticleBag(itemIds, subContentDef, localCache, isLive, counter);

            foreach (var fieldValue in fieldValues)
            {
                var id = fieldValue.Article.Id;
                var relatedId = fieldValue.RelatedItems.Any() ? fieldValue.RelatedItems.First() : 0;
                var res = new SingleArticleField
                {
                    Item = articleBag.TryGetValue(relatedId, out var article) ? article : null,
                    Aggregated = fieldValue.Field.Aggregated,
                    SubContentId = subContentDef.ContentId
                };
                var result = new KeyValuePair<int, ArticleField>(id, res);
                yield return result;
            }

        }

        private IEnumerable<KeyValuePair<int, ArticleField>> ProcessBackwardRelationField(int id, int[] articleIds, Dictionary<ArticleShapedByDefinitionKey, Article> localCache, bool isLive,
           ArticleCounter counter, string fieldName, Field fieldDef)
        {
            var subContentDef = ((BackwardRelationField) fieldDef).Content;

            if (subContentDef == null)
                throw new Exception(string.Format(ProductLoaderResources.ERR_XML_FIELD_MAP_NOT_EXISTS, 0,
                    "BackwardRelationField", id));

            var qpField = _fieldService.Read(id);

            var relatedArticlesIds = GetBackwardArticlesIds(qpField, articleIds);
            var flattenedRelatedArticlesIds = relatedArticlesIds.SelectMany(n => n.Value).Distinct().ToArray();

            var articleBag = GetArticleBag(flattenedRelatedArticlesIds, subContentDef, localCache, isLive, counter);

            var backwardFieldName = string.IsNullOrEmpty(fieldName) ? Guid.NewGuid().ToString() : fieldName;

            foreach (var articleId in articleIds)
            {
                var items = new Dictionary<int, Article>();
                if (relatedArticlesIds.TryGetValue(articleId, out var relatedIds))
                {
                    items = relatedIds
                        .Select(n => articleBag.TryGetValue(n, out var article) ? article : null)
                        .Where(n => n != null)
                        .ToDictionary(n => n.Id, m => m);

                }

                var res = new BackwardArticleField
                    {
                        Items = items,
                        FieldDisplayName = ((BackwardRelationField) fieldDef).DisplayName,
                        ContentId = subContentDef.ContentId,
                        SubContentId = subContentDef.ContentId,
                        FieldId = id,
                        FieldName = backwardFieldName
                    };

                    var result = new KeyValuePair<int, ArticleField>(articleId, res);
                    yield return result;

            }
        }

        private static IEnumerable<KeyValuePair<int, ArticleField>> ProcessSimpleFields(Qp8Bll.FieldValue[] fieldValues)
        {
            foreach (var fieldValue in fieldValues)
            {
                object nativeValue = fieldValue.ObjectValue;

                if (fieldValue.ObjectValue != null && fieldValue.Field.ExactType == FieldExactTypes.Numeric)
                {
                    if (fieldValue.Field.IsInteger && fieldValue.Field.IsLong)
                    {
                        nativeValue = Convert.ToInt64(fieldValue.ObjectValue);
                    }
                    else if ((fieldValue.Field.IsInteger && !fieldValue.Field.IsLong) ||
                             fieldValue.Field.RelationType == Qp8Bll.RelationType.OneToMany)
                    {
                        nativeValue = Convert.ToInt32(fieldValue.ObjectValue);
                    }
                    else if (fieldValue.Field.IsDecimal)
                    {
                        nativeValue = Convert.ToDecimal(fieldValue.ObjectValue);
                    }
                    else if (!fieldValue.Field.IsDecimal)
                    {
                        nativeValue = Convert.ToDouble(fieldValue.ObjectValue);
                    }
                }

                var res = new PlainArticleField
                {
                    Value = fieldValue.Value,
                    NativeValue = nativeValue,
                    PlainFieldType = (PlainFieldType) fieldValue.Field.ExactType, /*map to our types*/
                    DefaultValue = fieldValue.Field.DefaultValue
                };

                var result = new KeyValuePair<int, ArticleField>(fieldValue.Article.Id, res);

                yield return result;
            }
        }

        /// <summary>
        /// Получение идентификаторов статей которые связаны с данной по backward field
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, List<int>> GetBackwardArticlesIds(Qp8Bll.Field field, int[] articleIds)
        {
            var res = new Dictionary<int, List<int>>();

            if (field.RelationType == Qp8Bll.RelationType.ManyToMany && field.LinkId.HasValue)
            {
                var linkId = field.LinkId.Value;
                res = ArticleService.GetLinkedItems(new [] {linkId}, articleIds)[linkId];
            }
            else if (field.RelationType == Qp8Bll.RelationType.ManyToOne || field.RelationType == Qp8Bll.RelationType.OneToMany)
            {
                res = ArticleService.GetRelatedItems(new [] {field.Id}, articleIds)[field.Id];
            }

            return res;
        }
        #endregion

        #region Инициализация: формирование словарей, зависимостей контентов


        private class ArticleShapedByDefinitionKey
        {
            
            private static readonly Regex Re = new Regex(KEY_CACHE_GET_ARTICLE + @"_(\d+)");

            public bool IsLive { get; }

            public Content Definition { get; }

            public int ArticleId { get; }

            public ArticleShapedByDefinitionKey(int articleId, Content definition, bool isLive)
            {
                ArticleId = articleId;

                Definition = definition;

                IsLive = isLive;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is ArticleShapedByDefinitionKey otherKey))
                    return false;

                if (ReferenceEquals(this, otherKey))
                    return true;

                return IsLive == otherKey.IsLive &&
                       ArticleId == otherKey.ArticleId &&
                       Definition.Equals(otherKey.Definition);
            }

            [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
            public override int GetHashCode()
            {
                return HashHelper.CombineHashCodes(HashHelper.CombineHashCodes(IsLive.GetHashCode(), Definition.GetHashCode()), ArticleId.GetHashCode());
            }


            public static int Parse(string keyToLoad)
            {
                var m = Re.Match(keyToLoad);
                return int.Parse(m.Groups[1].Captures[0].Value);
            }
        }

        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        private Dictionary<ArticleShapedByDefinitionKey, Article> InitDictionaries(ProductDefinition productDefinition, bool isLive, ArticleCounter counter)
        {
            var dictionary = (Dictionaries)productDefinition.StorageSchema.Fields.SingleOrDefault(x => x is Dictionaries);
            var res = new Dictionary<ArticleShapedByDefinitionKey, Article>();
            if (dictionary == null || dictionary.ContentDictionaries.Count == 0)
            {
                return res;
            }

            const string keyCacheInitDic = "Dictionaries Cache for contentId={0}, isLive={1} hash={2}";
            var allContentsInDef = productDefinition.StorageSchema.GetChildContentsIncludingSelf();
            var allDicContentsSortedByLevel = allContentsInDef.Where(x => dictionary.ContentDictionaries.ContainsKey(x.ContentId)).OrderBy(x => x.GetChildContentsIncludingSelf().Length);

            foreach (var contentToCache in allDicContentsSortedByLevel)
            {
                var cachePeriod = dictionary.GetCachePeriodForContent(contentToCache.ContentId).Value;
                var cacheTags = GetTags(contentToCache);
                var itemsToAdd = _cacheProvider.GetOrAdd(string.Format(keyCacheInitDic, contentToCache.ContentId, isLive, contentToCache.GetHashCode()), cacheTags, cachePeriod, () =>
                {
                    var qpArticles = ArticleService.List(contentToCache.ContentId, null).ToArray();
                    var contentDic = new Dictionary<ArticleShapedByDefinitionKey, Article>();
                    var articles = GetArticlesForQpArticles(qpArticles, contentToCache, res, isLive, counter);

                    foreach (var article in articles)
                    {
                        if (article != null)
                        {
                            var dicKey = new ArticleShapedByDefinitionKey(article.Id, contentToCache, isLive);
                            contentDic[dicKey] = article;
                        }
                    }
                    
                    _logger.ForInfoEvent()
                        .Message(
                            "Dictionary (id={id}, isLive={isLive}, hash={hash}) has been filled with {count} articles", 
                            contentToCache.ContentId, isLive, contentToCache.GetHashCode(), articles.Length
                        )
                        .Property("name", contentToCache.ContentName)
                        .Log();
                    

                    return contentDic;
                });

                foreach (var itemFromCache in itemsToAdd)
                {
                    res[itemFromCache.Key] = itemFromCache.Value;
                }
            }

            return res;
        }
        #endregion

        #region Кэширование
        private static string[] GetTags(Content content)
        {
            return DpcContentInvalidator.GetTagNameByContentId(content.GetChildContentsIncludingSelf().Select(x => x.ContentId));
        }

        private static string GetArticleKeyStringForCache(ArticleShapedByDefinitionKey articleKey)
        {
            return string.Join("_", KEY_CACHE_GET_ARTICLE, articleKey.ArticleId, articleKey.IsLive, articleKey.Definition.GetHashCode());
        }
        #endregion

        #region Articles limitation
        private int GetArticlesLimit()
        {
            var limitText = _settingsService.GetSetting(SettingsTitles.PRODUCT_ARTICLES_LIMIT);

            if (int.TryParse(limitText, out var limit))
            {
                return limit;
            }
            return 0;
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

        private Qp8Bll.Article[] ReadArticles(int[] ids, bool isLive, Content contentDef = null)
        {
            var articles = ArticleService.List(contentDef?.ContentId ?? 0, ids)?.ToArray() ?? new Qp8Bll.Article[] {};

            
            if (contentDef != null && articles.Any())
            {
                var article = articles.First();
                if (article.ContentId != contentDef.ContentId)
                {
                    throw new Exception(string.Format(ProductLoaderResources.ERR_XML_CONTENTID_DOES_NOT_MATCH_EXPECTED, article.ContentId, article.Id, contentDef.ContentId));
                }
            }

            if (isLive)
            {
                articles = articles.Where(n => n.Status.Name == ARTICLE_STATUS_PUBLISHED).ToArray();
            }

            return articles;
        }
        #endregion
    }

    internal class ArticleCounter
    {
        private readonly ILogger _logger;
        private readonly int _articlesLimit;
        private readonly int _productId;
        private int _totalCount;
        private int _cacheCount;
        private int _hitCount;

        public ArticleCounter(int articlesLimit, ILogger logger, int productId)
        {
            _logger = logger;
            _articlesLimit = articlesLimit;
            _productId = productId;
            _totalCount = 0;
            _hitCount = 0;
            _cacheCount = 0;
        }

        public void CheckHitArticlesLimit(int[] ids)
        {
            if (IsActive())
            {
                _hitCount += ids.Length;
                CheckArticlesLimit(ids.Length);
            }
        }

        public void CheckCacheArticlesLimit(int[] ids)
        {
            if (IsActive())
            {
                _cacheCount += ids.Length;
                CheckArticlesLimit(ids.Length);
            }
        }

        public void LogCounter()
        {
            if (IsActive())
            {
                _logger.ForInfoEvent()
                    .Message("Article counting for product {id} completed", _productId)
                    .Property("totalCount", _totalCount)
                    .Property("cacheCount", _cacheCount)
                    .Property("hitCount", _hitCount)
                    .Log();
            }
        }

        private void CheckArticlesLimit(int increment = 1)
        {
            _totalCount += increment;
            if (IsExceeded())
            {
                LogCounter();
                throw new Exception($"Product is too big and not going to be processed. Max number of articles ({_articlesLimit}) limitation has been exceeded.");
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
    }

    internal class ExtensionContext
    {
        public int ArticleId { get; set; }
        public string ExtensionId { get; set; }
        public Qp8Bll.Article ExtensionArticle { get; set; }
    }
}
