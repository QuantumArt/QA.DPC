using QA.Core.Models.Configuration;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using QA.Core.Cache;
using QA.Core.DPC.Loader.Services;
using QA.Core.Logger;
using QA.Core.ProductCatalog.Actions.Exceptions;

namespace QA.Core.ProductCatalog.Actions
{
	public class CloneAction : ProductActionBase
	{
		private const string DoNotCloneArchiceKey = "DoNotCloneArchice";

		private readonly ICacheItemWatcher _cacheItemWatcher;
        protected int[] ClearFieldIds { get; private set; }

        public CloneAction(IArticleService articleService, IFieldService fieldService, IProductService productService, ILogger logger, Func<ITransaction> createTransaction, ICacheItemWatcher cacheItemWatcher)
			: base(articleService, fieldService, productService, logger, createTransaction)
		{
			_cacheItemWatcher = cacheItemWatcher;
		}

		#region Overrides
		protected override void ProcessProduct(int productId, Dictionary<string, string> actionParameters)
		{
            ClearFieldIds = actionParameters.GetClearFieldIds();

            _cacheItemWatcher.TrackChanges();

			ArticleService.LoadStructureCache();

			bool doNotCloneArticle = actionParameters.ContainsKey(DoNotCloneArchiceKey) && bool.Parse(actionParameters[DoNotCloneArchiceKey]);
			Func<Article, bool> filter = a => !doNotCloneArticle;

			var article = ArticleService.Read(productId);

			if (!ArticleService.CheckRelationSecurity(article.ContentId, new int[] { productId }, false)[productId])
				throw new ProductException(productId, "NoRelationAccess");

			var definition = Productservice.GetProductDefinition(0, article.ContentId);
			var dictionary = GetProductsToBeProcessed(article, definition, ef => ef.CloningMode, CloningMode.Copy, filter, true);

			PrepareProducts(dictionary);
			MapProducts(dictionary[productId], dictionary);
			SaveProducts(dictionary);
		}
		#endregion

		#region Private methods
		private static void PrepareProducts(Dictionary<int, Product<CloningMode>> dictionary)
		{
			foreach (var product in dictionary.Values)
			{
				product.Article.Id = 0;
			}
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

			SaveProduct(product);

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

		private void SaveProducts(Dictionary<int, Product<CloningMode>> dictionary)
		{
			foreach (var product in dictionary.Values)
			{
				if (product.Article.Id == 0)
				{
					SaveProduct(product);
				}
			}
		}

		private void SaveProduct(Product<CloningMode> product)
		{
			if (!product.Article.IsAggregated)
			{
				product.Article.PrepareForCopy(ClearFieldIds, ArticleClearType.EmptyValue);
				product.Article = ArticleService.Save(product.Article);
			}
		}
		#endregion
	}
}