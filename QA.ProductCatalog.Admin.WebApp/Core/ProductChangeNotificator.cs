using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using QA.Core;
using QA.Core.DPC.Loader.Services;
using QA.ProductCatalog.Admin.WebApp.Models;

namespace QA.ProductCatalog.Admin.WebApp.Core
{
	internal class ProductChangeNotificator : IProductChangeNotificator
	{
		private readonly List<IProductChangeSubscriber> _subscribers;
		private readonly ILogger _logger;

		public ProductChangeNotificator(ILogger logger)
		{
			_subscribers = new List<IProductChangeSubscriber>();

			_logger = logger;
		}

		public void AddSubscribers(IEnumerable<IProductChangeSubscriber> subscribers)
		{
			_subscribers.AddRange(subscribers);
		}

		private static Dictionary<int, ChangedValue> GetChangedFields(IEnumerable<XElement> newFields, IEnumerable<XElement> oldFields)
		{
			var changedFieldIds = new Dictionary<int, ChangedValue>();

			foreach (var newField in newFields)
			{
				int fieldId = int.Parse(newField.Attribute("id").Value);

				var oldField = oldFields.Single(x => x.Attribute("id").Value == fieldId.ToString());

				if (newField.Value != oldField.Value)
					changedFieldIds.Add(fieldId, new ChangedValue { OldValue = oldField.Value, NewValue = newField.Value });
			}
			return changedFieldIds;
		}

		public void NotifyProductsChanged(ArticleChangedNotification articleChangedNotification)
		{
			var articleDependencyService = ObjectFactoryBase.Resolve<IArticleDependencyService>();

			var oldArticle = XDocument.Parse(articleChangedNotification.Old_Xml).Root.Elements().First();

			var newArticle = XDocument.Parse(articleChangedNotification.New_Xml).Root.Elements().First();

			int articleId = int.Parse(newArticle.Attribute("id").Value);

			var newFields = newArticle.Elements("customFields").First().Elements("field");

			var oldFields = oldArticle.Elements("customFields").First().Elements("field");

			var changedFields = GetChangedFields(newFields, oldFields);

			var affectedProductIdsByContentId = changedFields.Any()
				? articleDependencyService.GetAffectedProducts(articleId, changedFields)
				: new Dictionary<int, int[]>();

			var newExtensions = newArticle.Elements("extensions").First().Elements("extension");

			var oldExtensions = oldArticle.Elements("extensions").First().Elements("extension");

			foreach (var newExtension in newExtensions)
			{
				int typeId = int.Parse(newExtension.Attribute("typeId").Value);

				int extensionArticleId = int.Parse(newExtension.Attribute("id").Value);

				var newExtensionFields = newExtension.Elements("customFields").First().Elements("field");

				var oldExtensionFields = oldExtensions.Single(x => x.Attribute("typeId").Value == typeId.ToString()).Elements("customFields").First().Elements("field");

				var changedExtensionFields = GetChangedFields(newExtensionFields, oldExtensionFields);

				if (changedExtensionFields.Count > 0)
				{
					foreach (var affectedProductsInContent in articleDependencyService.GetAffectedProducts(extensionArticleId, changedExtensionFields))
					{
						if (!affectedProductIdsByContentId.ContainsKey(affectedProductsInContent.Key))
							affectedProductIdsByContentId[affectedProductsInContent.Key] = affectedProductsInContent.Value;
						else
							affectedProductIdsByContentId[affectedProductsInContent.Key] = affectedProductIdsByContentId[affectedProductsInContent.Key].Union(affectedProductsInContent.Value).ToArray();
					}
				}
			}

			foreach (var productChangeSubscriber in _subscribers)
				try
				{
					productChangeSubscriber.NotifyProductsChanged(affectedProductIdsByContentId);
				}
				catch (Exception ex)
				{
					_logger.ErrorException("Ошибка при запуске " + productChangeSubscriber.GetType().Name, ex);
				}
				
		}
	}
}