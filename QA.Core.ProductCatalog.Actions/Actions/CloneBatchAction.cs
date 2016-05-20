﻿using System;
using System.Collections.Generic;
using System.Linq;
using QA.Core.Cache;
using QA.Core.DPC.Loader.Services;
using QA.Core.Models.Configuration;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API;

namespace QA.Core.ProductCatalog.Actions
{
	public class CloneBatchAction : ActionBase
	{
		private const string FieldIdParameterKey = "FieldId";
		private const string ArticleIdParameterKey = "ArticleId";

		private ICacheItemWatcher _cacheItemWatcher;
		public List<int> Ids { get; private set; }

        public CloneBatchAction(IArticleService articleService, IFieldService fieldService, IProductService productService, ILogger logger, Func<ITransaction> createTransaction, ICacheItemWatcher cacheItemWatcher)
			: base(articleService, fieldService, productService, logger, createTransaction)
		{
			_cacheItemWatcher = cacheItemWatcher;
			Ids = new List<int>();
		}

		#region Overrides
		protected override void OnStartProcess()
		{
			Ids.Clear();
		}

		protected override void ProcessProduct(int productId, Dictionary<string, string> actionParameters)
		{
            var clearFieldIds = actionParameters.GetClearFieldIds();

            _cacheItemWatcher.TrackChanges();

			ArticleService.LoadStructureCache();

			var article = ArticleService.Read(productId);

			if (Filter(article))
			{
				if (!ArticleService.CheckRelationSecurity(article.ContentId, new int[] { productId }, false)[productId])
					throw new ProductException(productId, "Операция недопустима из-за недостаточных прав доступа по связям");

				var definition = Productservice.GetProductDefinition(0, article.ContentId);
				UpdateDefinition(article, definition, actionParameters);

				var dictionary = GetProductsToBeProcessed<CloningMode>(article, definition, ef => ef.CloningMode, CloningMode.Copy, Filter);
				var missedAggArticles = PrepareProducts(dictionary, clearFieldIds);
				MapProducts(dictionary[productId], dictionary);
				int id = SaveProducts(productId, dictionary, missedAggArticles);
				Ids.Add(id);
			}
		}
		#endregion		

		#region Private methods
		private static bool Filter(Article article)
		{
			return !article.Archived;
		}

		private static void UpdateDefinition(Article product, ProductDefinition definition, Dictionary<string, string> actionParameters)
		{
			string fieldIdString;

			if (actionParameters.TryGetValue(FieldIdParameterKey, out fieldIdString))
			{
				int fieldId = int.Parse(fieldIdString);

				var articleDefinition = definition.StorageSchema.Fields.OfType<Association>().FirstOrDefault(f => f.FieldId == fieldId);
				
				if (articleDefinition != null)
				{					
					string articleIdString;

					if (actionParameters.TryGetValue(ArticleIdParameterKey, out articleIdString))
					{
						articleDefinition.CloningMode = CloningMode.UseExisting;
						int articleId = int.Parse(articleIdString);
						product.FieldValues.Find(fv => fv.Field.Id == fieldId).Value = articleId.ToString();
					}
					else
					{
						articleDefinition.CloningMode = CloningMode.Ignore;
					}
				}
			}		
		}

		private static List<Article> PrepareProducts(Dictionary<int, Product<CloningMode>> dictionary, int[] clearFieldIds)
		{
			int id = 0;

			List<Article> missedArticles = new List<Article>();

			foreach (var product in dictionary.Values)
			{
				product.Article.ClearOldIds();
                product.Article.ClearFields(clearFieldIds, ArticleClearType.DefaultValue);
            }

			foreach (var product in dictionary.Values)
			{
				product.Article.Id = --id;

			}

			foreach (var product in dictionary.Values)
			{
				foreach (var aggArticle in product.Article.AggregatedArticles)
				{
					if (aggArticle.Id > 0)
					{
						var temp = aggArticle.FieldValues; // force load
						aggArticle.Id = --id;
						missedArticles.Add(aggArticle);

					}
				}
			}

			return missedArticles;


		}

		private void MapProducts(Product<CloningMode> product, Dictionary<int, Product<CloningMode>> dictionary)
		{
			bool isProcecced = product.IsProcessed;
			product.IsProcessed = true;

			var backwardFields = (from fv in product.Article.FieldValues
								  where fv.Field.RelationType == RelationType.ManyToOne
								  select new FieldValue()
								  {
									  Field = fv.Field,
									  Value = fv.Value
								  }).ToArray();

			if (!isProcecced)
			{
				MapForwardProducts(product, dictionary);
			}

			if (!isProcecced)
			{
				MapBackwardProducts(product, backwardFields, dictionary);
			}
		}

		private void MapForwardProducts(Product<CloningMode> product, Dictionary<int, Product<CloningMode>> dictionary)
		{
			var fields = product.Article.FieldValues.Where(fv => fv.Field.RelationType == RelationType.ManyToMany || fv.Field.RelationType == RelationType.OneToMany);

			foreach (var fv in fields)
			{
				var mode = product.GetCloningMode(fv.Field);
				bool isBackward = product.BackwardMap[fv.Field.Id];

				if (!isBackward)
				{
					switch (mode)
					{
						case CloningMode.Copy:
							MapForwardField(fv, dictionary);
							break;
						case CloningMode.Ignore:
							fv.UpdateValue(string.Empty);
							break;
						case null:
							fv.UpdateValue(string.Empty);
							break;
					}
				}
			}
		}

		private void MapForwardField(FieldValue fv, Dictionary<int, Product<CloningMode>> dictionary)
		{
			var products = fv.RelatedItems.Where(id => dictionary.ContainsKey(id)).Select(id => dictionary[id]);
			var idsForIgnore = fv.RelatedItems.Where(id => !dictionary.ContainsKey(id));

			foreach (var p in products)
			{
				MapProducts(p, dictionary);
			}

			var ids = products.Select(p => p.Article.Id).Concat(idsForIgnore);
			fv.UpdateValue(string.Join(",", ids));
		}

		private void MapBackwardProducts(Product<CloningMode> product, IEnumerable<FieldValue> fields, Dictionary<int, Product<CloningMode>> dictionary)
		{
			string backwardId = product.Article.Id.ToString();			

			foreach (var fv in fields)
			{
				var mode = product.GetCloningMode(fv.Field);

				switch (mode)
				{
					case CloningMode.Copy:
						MapBackwardField(backwardId, fv, fv.Field.BackRelationId.Value, dictionary);
						break;
					case CloningMode.Ignore:
						break;
					case null:
						break;
				}
			}

			foreach (var fv in product.BackwardFieldValues)
			{
				var mode = product.GetCloningMode(fv.Field);

				if (mode == CloningMode.Copy)
				{
					MapBackwardField(backwardId, fv, fv.Field.Id, dictionary);
				}
			}
		}

		private void MapBackwardField(string backwardId, FieldValue fv, int fieldId, Dictionary<int, Product<CloningMode>> dictionary)
		{
			var products = fv.RelatedItems.Where(id => dictionary.ContainsKey(id)).Select(id => dictionary[id]);

			foreach (var p in products)
			{
				var backwardFieldValue = p.Article.FieldValues.Find(v => v.Field.Id == fieldId);
				var ids = new List<int>();

				foreach (var id in backwardFieldValue.RelatedItems)
				{
					if (dictionary.ContainsKey(id))
					{
						var newId = dictionary[id].Article.Id;

						if (newId == 0)
						{
							ids.Add(id);
						}
						else
						{
							ids.Add(newId);
						}
					}
					else
					{
						ids.Add(id);
					}
				}

				var backwardValue = string.Join(",", ids);

				backwardFieldValue.UpdateValue(backwardValue);
				MapProducts(p, dictionary);
			}
		}      

        private int SaveProducts(int productId, Dictionary<int, Product<CloningMode>> dictionary, List<Article> missedAggArticles)
		{
			foreach (var a in missedAggArticles)
			{
				FieldValue fieldValue = a.FieldValues.Single(fv => fv.Field.Aggregated);
				int oldId = Int32.Parse(fieldValue.Value);
				fieldValue.Value = dictionary[oldId].Article.Id.ToString();
			}
			var articles = dictionary.Values.Select(p => p.Article).Union(missedAggArticles).ToArray();

            var result = ArticleService.BatchUpdate(articles);
			return result.First().CreatedArticleId;
		}

		#endregion
	}
}