﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using NLog;
using NLog.Fluent;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.Resources;
using QA.Core.Models.Configuration;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;

namespace QA.Core.ProductCatalog.Actions.Actions.Abstract
{
	public abstract class ProductActionBase : ActionTaskBase
	{
		#region Constants
		private const string LoggerMessage = "Can't process product: ";
		#endregion

		#region Protected properties
		protected IArticleService ArticleService { get; private set; }
		protected IFieldService FieldService { get; private set; }
		protected IProductService Productservice { get; private set; }		
		protected Func<ITransaction> CreateTransaction { get; private set; }
		
		protected ResourceManager ResourceManager { get; private set; } = new ResourceManager(typeof(TaskStrings));

		protected int UserId;

		protected string UserName;

		#endregion

		#region Constructors
		protected ProductActionBase(IArticleService articleService, IFieldService fieldService, IProductService productservice, Func<ITransaction> createTransaction)
			: base()
		{
			if (articleService == null)
				throw new ArgumentNullException("articleService");

			if (productservice == null)
				throw new ArgumentNullException("productservice");

			if (createTransaction == null)
				throw new ArgumentNullException("transactionFactory");

			ArticleService = articleService;
			FieldService = fieldService;
			Productservice = productservice;

			CreateTransaction = createTransaction;
		}
		#endregion

		#region IAction implementation
		public override ActionTaskResult Process(ActionContext context)
		{
			if (context == null)
				throw new ArgumentNullException("context");

			if (context.ContentItemIds == null || context.ContentItemIds.Length == 0)
				throw new ArgumentException("ContentItemIds cant be empty", "context.ContentItemIds");

			UserId = context.UserId;
			UserName = context.UserName;
			
			OnStartProcess();

			var exceptions = new List<ProductException>();
			int index = 0;	

			foreach (int id in context.ContentItemIds)
			{
				if (TaskContext.IsCancellationRequested)
				{
					TaskContext.IsCancelled = true;
					break;
				}

				try
				{
					using (var transaction = CreateTransaction())
					{
						ProcessProduct(id, context.Parameters ?? new Dictionary<string, string>());						
						transaction.Commit();
					}

					byte progress = (byte)(++index * 100 / context.ContentItemIds.Length);
					TaskContext.SetProgress(progress);
				}
				catch (ProductException pex)
				{
					var logLevel = pex.IsError ? LogLevel.Error : LogLevel.Info;
					var builder = Logger.ForLogEvent(logLevel).Message(LoggerMessage + id);
					var result = ActionTaskResult.FromString(pex.Message);
					var msg = ResourceManager.GetString(pex.Message);
					if (result != null)
					{
						builder.Property("taskResult", result.ToString());
					}
					else if (msg != null)
					{
						builder.Property("taskResult", string.Format(msg, id));						
					}
					else
					{
						builder.Exception(pex);
					}
					builder.Log();
					exceptions.Add(pex);
				}
				catch (AggregateException aex)
				{
					foreach(var iex in aex.InnerExceptions)
					{
						var ipex = iex as ProductException;

						if (ipex == null)
						{
							ipex = new ProductException(id, nameof(TaskStrings.ActionErrorMessage), iex);
						}
						Logger.ForErrorEvent().Message(LoggerMessage + id).Exception(ipex).Log();
						exceptions.Add(ipex);
					}
				}
				catch (Exception ex)
				{
					Logger.ForErrorEvent().Message(LoggerMessage + id).Exception(ex).Log();
					exceptions.Add(new ProductException(id, nameof(TaskStrings.ServerError), ex));
				}
			}

            try
            {
                OnEndProcess();
            }
            catch(Exception ex)
            {
                exceptions.Add(new ProductException(0, "OnEndProcess error", ex));
            }
            
            if (exceptions.Any())
			{
				throw new ActionException(TaskStrings.ActionErrorMessage, exceptions, context);
			}
     
            return null;
		}
	    #endregion

		#region Abstract methods
		protected abstract void ProcessProduct(int productId, Dictionary<string, string> actionParameters);
		#endregion

		#region Protected methods
		protected virtual void OnStartProcess()
		{
		}

        protected virtual void OnEndProcess()
        {
        }
        protected Dictionary<int, Product<T>> GetProductsToBeProcessed<T>(
			Article source,
			ProductDefinition definition,
			Func<Association, T> selectMode,
			T mode,
			bool excludeArchive
            )
			where T : struct
		{
			return GetProductsToBeProcessed<T>(source, definition, selectMode, mode, arcicle => true, excludeArchive);
		}

		protected Dictionary<int, Product<T>> GetProductsToBeProcessed<T>(
			Article source,
			ProductDefinition definition,
			Func<Association, T> selectMode,
			T mode,
			Func<Article, bool> filter,
            bool excludeArchive
		    )
			where T : struct
		{
			var dictionary = new Dictionary<int, Product<T>>();

			GetProductsToBeProcessed(source, definition.StorageSchema, dictionary, selectMode, mode, filter, excludeArchive);

			return dictionary;
		}


		protected static void ValidateMessageResult(int productId, MessageResult result)
		{
			if (result != null && (result.Type == ActionMessageType.Error || ( result.FailedIds != null && result.FailedIds.Any())))
			{
				throw new MessageResultException(productId, result.Text, result);
			}
		}

		#endregion

		#region Private methods
		private void GetProductsToBeProcessed<T>(
			Article article,
			Models.Configuration.Content definition,
			Dictionary<int, Product<T>> dictionary,
			Func<Association, T> selectMode,
			T mode,
			Func<Article, bool> filter,
			bool excludeArchive
		    )
			where T : struct
		{
							
				var entityFields = definition == null ? new Association[0] : definition.Fields.OfType<Association>().ToArray();
				var fieldDefinitions = entityFields.ToDictionary(f => f.FieldId, ContentSelector<T>);
				var fieldModes = entityFields.ToDictionary(f => f.FieldId, selectMode);
				var backwardFieldValues = GetBackwardFieldValues(article, entityFields.OfType<BackwardRelationField>(), excludeArchive);

				var product = new Product<T>(article, fieldModes, backwardFieldValues);

				if (!dictionary.ContainsKey(article.Id))
				{
					dictionary.Add(article.Id, product);
				}
				
				var root = dictionary.Values.First().Article;

				var relatedFields = (from fv in article.FieldValues.Concat(backwardFieldValues)
									 join ef in entityFields on fv.Field.Id equals ef.FieldId
									 where selectMode(ef).Equals(mode)
                                     select fv)
									 .ToArray();

				//LogFieldValues(article.FieldValues, "ContentId = " + article.ContentId + " IsAggregated = " + article.IsAggregated);
	
				var relatedItems = (from fv in relatedFields
									from id in fv.RelatedItems
									where !dictionary.ContainsKey(id) && fv.Field.ExactType != FieldExactTypes.Classifier
                                    let f = fv.Field
									let contentId = backwardFieldValues.Contains(fv) ? f.ContentId : f.RelateToContentId ?? 0
									group new { Id = id, FieldId = f.Id, Relation = f.RelationType } by contentId into g
									select new { ContentId = g.Key, RelatedItems = g })
									.ToArray();

				foreach (var item in relatedItems)
				{
					var ids = item.RelatedItems.Select(itm => itm.Id).Distinct().ToArray();
					var articleMap = ArticleService.List(item.ContentId, ids, excludeArchive).ToDictionary(a => a.Id);

					foreach (var relatedItem in item.RelatedItems)
					{
					    if (articleMap.TryGetValue(relatedItem.Id, out var relatedArticle) && filter(relatedArticle))
					    {

					        Models.Configuration.Content def = null;
					        fieldDefinitions.TryGetValue(relatedItem.FieldId, out def);

					        if (relatedItem.Relation != RelationType.ManyToMany || relatedArticle.ContentId != root.ContentId)
					        {
					            GetProductsToBeProcessed(relatedArticle, def, dictionary, selectMode, mode, filter, excludeArchive);

					            var relatedProduct = dictionary[relatedArticle.Id];

					            var backwardFields = from fv in relatedFields
					                where fv.Field.RelationType == RelationType.ManyToOne
					                join pfv in relatedProduct.Article.FieldValues on fv.Field.BackRelationId equals pfv.Field.Id
					                select pfv;

					            foreach (var fv in backwardFields)
					            {
					                relatedProduct.BackwardMap[fv.Field.Id] = true;
					            }
					        }
					    }
					}
				}

				foreach (var fv in backwardFieldValues)
				{
					var currentMode = product.GetCloningMode(fv.Field);

					if (currentMode.HasValue && currentMode.Value.Equals(mode))
					{
						var products = fv.RelatedItems.Where(id => dictionary.ContainsKey(id)).Select(id => dictionary[id]);

						foreach (var p in products)
						{
							p.BackwardMap[fv.Field.Id] = true;
						}
					}
				}

				var aggregatedArticles = product.Article.AggregatedArticles.ToArray();

				for (int i = 0; i < aggregatedArticles.Length; i++)
				{
					int id = aggregatedArticles[i].Id;

					if (dictionary.ContainsKey(id))
					{
						aggregatedArticles[i] = dictionary[id].Article;
					}

				}

				product.Article.AggregatedArticles = aggregatedArticles;
		}	

		private List<FieldValue> GetBackwardFieldValues(Article article, IEnumerable<BackwardRelationField> fields, bool excludeArchive)
		{
			var fieldValues = new List<FieldValue>();

			foreach (var field in fields)
			{
				var articleField = FieldService.Read(field.FieldId);
				var value = GetBackwardValue(articleField, article.Id, excludeArchive);

				var fv = new FieldValue();
				fv.Article = article;
				fv.Field = articleField;
				fv.UpdateValue(value);

				fieldValues.Add(fv);
			}

			return fieldValues;
		}

		private string GetBackwardValue(Quantumart.QP8.BLL.Field field, int articleId, bool excludeArchive)
		{
			string res = string.Empty;

			if (field.RelationType == RelationType.ManyToMany && field.LinkId.HasValue)
			{
				res = ArticleService.GetLinkedItems(field.LinkId.Value, articleId, excludeArchive);
			}
			else if (field.RelationType == RelationType.ManyToOne || field.RelationType == RelationType.OneToMany)
			{
				res = ArticleService.GetRelatedItems(field.Id, articleId, excludeArchive);
			}

			return res;
		}

		private static Models.Configuration.Content ContentSelector<T>(Association field)
		{
		    EntityField entityField = field as EntityField;

		    if (entityField == null)
		        return null;

		    if (typeof (T) == typeof (CloningMode))
		        return entityField.CloneDefinition ?? entityField.Content;
		    else
		        return entityField.Content;
		}

		protected void LogFieldValues(IEnumerable<FieldValue> fieldValues, string header = "")
		{
			var sb = new StringBuilder(header);
			sb.AppendLine();

			foreach (var fv in fieldValues)
			{
				int backwardId = fv.Field.BackwardField == null ? 0 : fv.Field.BackwardField.Id;
				int backRelationId = fv.Field.BackRelationId.HasValue ? fv.Field.BackRelationId.Value : 0;
				var f = fv.Field;
				sb.AppendFormat("id = {0}, name = {1}, val = {2}, r = {3}, backwardId = {4}, backRelationId = {5}, linkId = {6}, Aggregated = {7}, IsClassifier = {8}, ClassifierId = {9}", f.Id, f.Name, fv.Value, f.RelationType, backwardId, backRelationId, f.LinkId, f.Aggregated, f.IsClassifier, f.ClassifierId);
				sb.AppendLine();
			}

			Logger.Info(sb.ToString());
		}
		#endregion
	}
}