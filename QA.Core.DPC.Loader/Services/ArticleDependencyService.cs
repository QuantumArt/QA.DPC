using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QA.Core.Cache;
using QA.Core.DPC.QP.Services;
using QA.Core.Models.Configuration;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QP8.Utils;
using Content = QA.Core.Models.Configuration.Content;
using Field = QA.Core.Models.Configuration.Field;
using QA.Core.DPC.QP.Servives;

namespace QA.Core.DPC.Loader.Services
{
	public class ArticleDependencyService : IArticleDependencyService
	{
		private readonly IContentDefinitionService _contentDefinitionService;
		private readonly string _connectionString;
		private readonly IVersionedCacheProvider _cacheProvider;
		private readonly int _prodDefContentId;
		private readonly ArticleService _articleService;
		private readonly FieldService _fieldService;

		public ArticleDependencyService(IContentDefinitionService contentDefinitionService, IServiceFactory serviceFactory, IVersionedCacheProvider cacheProvider, ISettingsService settingsService, IConnectionProvider connectionProvider)
		{
			_contentDefinitionService = contentDefinitionService;

			_connectionString = connectionProvider.GetConnection();

			_cacheProvider = cacheProvider;

			_prodDefContentId = int.Parse(settingsService.GetSetting(SettingsTitles.PRODUCT_DEFINITIONS_CONTENT_ID));

			_articleService = serviceFactory.GetArticleService();

			_fieldService = serviceFactory.GetFieldService();
		}


		private class PathToRootContent
		{
			/// <summary>
			/// contentId корня
			/// </summary>
			public int ContentId;

			public Field[] PathFields;

			public PathToRootContent(int contentId, Field[] pathFields)
			{
				ContentId = contentId;

				PathFields = pathFields;
			}
		}

		private class DefinitionsFields
		{
			/// <summary>
			/// поля, кроме MonitoredBackwardRelationFields, которые есть часть продукта, fieldId - ключ словаря
			/// в значении словаря список путей к корню
			/// </summary>
			public readonly Dictionary<int, List<PathToRootContent>> MonitoredFieldIds = new Dictionary<int, List<PathToRootContent>>();

			public readonly Dictionary<int, List<PathToRootContent>> ContentIdsDependentOfAllPlainFields = new Dictionary<int, List<PathToRootContent>>();

			public readonly Dictionary<int, List<PathToRootContent>> MonitoredBackwardRelationFields = new Dictionary<int, List<PathToRootContent>>();
		}


		/// <summary>
		/// возвращает словарь куда входят все Field входящие в продукты и hashset их родительских Field или null если данный Field в корневом Article продукта
		/// </summary>
		/// <param name="definitions"></param>
		/// <returns></returns>
		private DefinitionsFields GetDefinitionsFields(IEnumerable<Content> definitions)
		{
			var definitionsFields = new DefinitionsFields();

			foreach (var definition in definitions)
				FillDefinitionsFields(definitionsFields, definition, new Field[] { }, definition.ContentId);

			return definitionsFields;
		}

		private void FillDefinitionsFields(DefinitionsFields definitionsFields, Content definition, Field[] parentFields, int rootContentId)
		{
			if (definition.LoadAllPlainFields)
			{
				if (!definitionsFields.ContentIdsDependentOfAllPlainFields.ContainsKey(definition.ContentId))
					definitionsFields.ContentIdsDependentOfAllPlainFields[definition.ContentId] = new List<PathToRootContent>();

				definitionsFields.ContentIdsDependentOfAllPlainFields[definition.ContentId].Add(new PathToRootContent(rootContentId, parentFields)); 
			}

			foreach (var field in definition.Fields)
			{
				if (field is BackwardRelationField)
				{
					if (!definitionsFields.MonitoredBackwardRelationFields.ContainsKey(field.FieldId))
						definitionsFields.MonitoredBackwardRelationFields[field.FieldId] = new List<PathToRootContent>();

					definitionsFields.MonitoredBackwardRelationFields[field.FieldId].Add(new PathToRootContent(rootContentId, parentFields)); 
				}
				else
				{
					if (!definitionsFields.MonitoredFieldIds.ContainsKey(field.FieldId))
						definitionsFields.MonitoredFieldIds[field.FieldId] = new List<PathToRootContent>();

					definitionsFields.MonitoredFieldIds[field.FieldId].Add(new PathToRootContent(rootContentId, parentFields)); 
				}

				var childDefinitionParentFields = new[] { field }.Concat(parentFields).ToArray();

				if (field is BackwardRelationField)
					FillDefinitionsFields(definitionsFields, ((BackwardRelationField)field).Content, childDefinitionParentFields, rootContentId);
				else
					if (field is EntityField)
						FillDefinitionsFields(definitionsFields, ((EntityField)field).Content, childDefinitionParentFields, rootContentId);
					else if (field is ExtensionField)
						foreach (var extFieldDefinition in ((ExtensionField)field).ContentMapping.Select(x => x.Value))
							FillDefinitionsFields(definitionsFields, extFieldDefinition, childDefinitionParentFields, rootContentId);
			}
		}



		public Dictionary<int,int[]> GetAffectedProducts(int articleId, Dictionary<int, ChangedValue> changedFields, bool isLive = false, int? definitionId = null)
		{
			var allDefinitionsFields = GetAllDefinitionsFields(isLive, definitionId);

			var affectedProductIdsByContentId = new Dictionary<int, List<int>>();

			using (new QPConnectionScope(_connectionString))
			{
				int contentId = _articleService.Read(articleId).ContentId;

				var contentFields = _fieldService.List(contentId).Where(x => changedFields.ContainsKey(x.Id)).ToArray();

				var rootPathsToTrack = new Dictionary<int, List<PathToRootContent>>();

				var linkFieldsWithBackFieldsToTrack = contentFields.Where(x => x.RelationType != RelationType.None);

				foreach (var contentField in linkFieldsWithBackFieldsToTrack)
				{
					List<PathToRootContent> paths = null;

					//если есть обратная связь то тоже нужно искать зависимости по добавленным и удаленным значениям
					if (contentField.BackRelationId.HasValue && allDefinitionsFields.MonitoredFieldIds.ContainsKey(contentField.BackRelationId.Value))
						paths = allDefinitionsFields.MonitoredFieldIds[contentField.BackRelationId.Value];
					else if (allDefinitionsFields.MonitoredBackwardRelationFields.ContainsKey(contentField.Id))
						paths = allDefinitionsFields.MonitoredBackwardRelationFields[contentField.Id];

					if (paths == null)
						continue;

					int[] changedIds = GetChangedIds(changedFields[contentField.Id]);

					foreach (var changedArticleId in changedIds)
					{
						if (!rootPathsToTrack.ContainsKey(changedArticleId))
							rootPathsToTrack[changedArticleId] = new List<PathToRootContent>();

						rootPathsToTrack[changedArticleId].AddRange(paths);
					}
				}

				//изменили plain поле а мы по этому контенту все plain поля мониторим
				if (allDefinitionsFields.ContentIdsDependentOfAllPlainFields.ContainsKey(contentId) && contentFields.Any(x => x.RelationType == RelationType.None))
				{
					if (!rootPathsToTrack.ContainsKey(articleId))
						rootPathsToTrack[articleId] = new List<PathToRootContent>();

					rootPathsToTrack[articleId].AddRange(allDefinitionsFields.ContentIdsDependentOfAllPlainFields[contentId]);
				}


				foreach (int changedFieldId in changedFields.Keys)
				{
					if (allDefinitionsFields.MonitoredFieldIds.ContainsKey(changedFieldId))
					{
						if (!rootPathsToTrack.ContainsKey(articleId))
							rootPathsToTrack[articleId] = new List<PathToRootContent>();

						rootPathsToTrack[articleId].AddRange(allDefinitionsFields.MonitoredFieldIds[changedFieldId]);
					}
				}

				foreach (var articleToTrackKV in rootPathsToTrack)
				{
					var paths = articleToTrackKV.Value;

					var pathDistinct = paths.Distinct(new PathToRootCompaper());

					foreach (var path in pathDistinct)
					{
						int rootContentId = path.ContentId;

						if (!affectedProductIdsByContentId.ContainsKey(rootContentId))
							affectedProductIdsByContentId[rootContentId] = new List<int>();

						if (path.PathFields.Length == 0)
							affectedProductIdsByContentId[rootContentId].Add(articleToTrackKV.Key);
						else
							affectedProductIdsByContentId[rootContentId].AddRange(GetRootArticleIds(path.PathFields, articleToTrackKV.Key));
					}
				}
			}

			return affectedProductIdsByContentId
				.Where(x => x.Value.Count > 0)
				.ToDictionary(x => x.Key, x => x.Value.Distinct().ToArray());
		}

		class PathToRootCompaper : IEqualityComparer<PathToRootContent>
		{
			public bool Equals(PathToRootContent x, PathToRootContent y)
			{
				return x.ContentId == y.ContentId && ((IStructuralEquatable) x.PathFields).Equals(y.PathFields, EqualityComparer<object>.Default);
			}

			public int GetHashCode(PathToRootContent obj)
			{
				return (obj.ContentId.ToString() + ((IStructuralEquatable) obj.PathFields).GetHashCode(EqualityComparer<object>.Default)).GetHashCode();
			}
		}

		private static int[] GetChangedIds(ChangedValue values)
		{
			int[] oldIds = values.OldValue.Split(new[] { ',' }).Select(int.Parse).ToArray();

			int[] newIds = values.NewValue.Split(new[] { ',' }).Select(int.Parse).ToArray();

			return newIds.Except(oldIds).Concat(oldIds.Except(newIds)).ToArray();
		}

		private DefinitionsFields GetAllDefinitionsFields(bool isLive, int? definitionId)
		{
			string cacheKey = "AllDefinitionsFields" + isLive + definitionId;

			return _cacheProvider.GetOrAdd(
				cacheKey,
				new[] { _prodDefContentId.ToString() },
				TimeSpan.FromHours(3),
				() => GetDefinitionsFields(_contentDefinitionService.GetDefinitions(isLive).Where(x => !definitionId.HasValue || x.ContentId == definitionId)));
		}

		private int[] GetRootArticleIds(IEnumerable<Field> parentFields, int articleId)
		{
			var currentArticleIds = new[] { articleId };

			int contentId = _articleService.Read(articleId).ContentId;

			foreach (var field in parentFields)
				currentArticleIds = GetParentArticleIds(contentId, currentArticleIds, field, out contentId);

			return currentArticleIds;
		}


		private int[] GetManyToManyLinkedItems(int? linkId, int[] articleIds)
		{
			return linkId.HasValue
				? _articleService.GetLinkedItemsMultiple(linkId.Value, articleIds)
					.SelectMany(x => Converter.ToIdArray(x.Value))
					.Distinct()
					.ToArray()
				: new int[0];
		}

		private int[] GetParentArticleIds(int childContentId, int[] childArticleIds, Field parentField, out int parentsContentId)
		{
			if (childArticleIds == null || childArticleIds.Length == 0)
			{
				parentsContentId = 0;

				return new int[0];
			}

			if (parentField is BackwardRelationField)
			{
				var childArticleField = _fieldService.Read(parentField.FieldId);

				if (!childArticleField.RelateToContentId.HasValue)
					throw new Exception("Некорректный BackwardRelationField: RelateToContentId у fieldId=" + childArticleField.Id + " null");

				parentsContentId = childArticleField.RelateToContentId.Value;

				if (childArticleField.RelationType == RelationType.ManyToMany)
				{
					return GetManyToManyLinkedItems(childArticleField.LinkId, childArticleIds);
				}
				else if (childArticleField.RelationType == RelationType.OneToMany)
				{
					return _articleService.GetFieldValues(childArticleIds, childContentId, childArticleField.Name)
						.Where(x => !string.IsNullOrEmpty(x))
						.Select(int.Parse)
						.Distinct()
						.ToArray();
				}
				else
					throw new Exception("Некорректный BackwardRelationField с ID=" + parentField.FieldId);
			}
			else if (parentField is ExtensionField)
			{
				var parentArticleField = _fieldService.Read(parentField.FieldId);

				parentsContentId = parentArticleField.ContentId;

				var childField = _fieldService.List(childContentId).Single(x => x.ClassifierId == parentField.FieldId);

				return _articleService.GetFieldValues(childArticleIds, childContentId, childField.Name)
					.Where(x => !string.IsNullOrEmpty(x))
					.Select(int.Parse)
					.ToArray();
			}
			else if (parentField is EntityField)
			{
				var parentArticleField = _fieldService.Read(parentField.FieldId);

				parentsContentId = parentArticleField.ContentId;

				switch (parentArticleField.RelationType)
				{
					case RelationType.OneToMany:

						return _articleService.GetRelatedItemsMultiple(parentField.FieldId, childArticleIds)
							.SelectMany(x => Converter.ToIdArray(x.Value))
							.Distinct()
							.ToArray();

					case RelationType.ManyToMany:

						return GetManyToManyLinkedItems(parentArticleField.LinkId, childArticleIds);

					case RelationType.ManyToOne:

						if (!parentArticleField.BackRelationId.HasValue)
							throw new Exception("Связь ManyToMany некорректно настроена у fieldId=" + parentField.FieldId + ": BackRelationId null");

						return _articleService.GetFieldValues(childArticleIds, childContentId, parentArticleField.BackRelationId.Value)
							.Where(x => !string.IsNullOrEmpty(x))
							.Distinct()
							.Select(int.Parse)
							.ToArray();

					default:
						throw new Exception("Связь типа " + parentArticleField.RelationType + " не поддерживается");
				}
			}
			else
				throw new Exception("Тип поля " + parentField.GetType().Name + " не поддерживается");
		}

		internal void InitCache(bool isLive)
		{
			GetAllDefinitionsFields(isLive, null);
		}
	}
}
