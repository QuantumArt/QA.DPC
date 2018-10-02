using System;
using System.Collections.Generic;
using System.Linq;
using QA.Core.Models.Configuration;
using QA.Core.ProductCatalog.Actions.Services;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API;
using Content = QA.Core.Models.Configuration.Content;
using Field = QA.Core.Models.Configuration.Field;

namespace QA.Core.DPC.Loader.Services
{
    public class DefinitionEditorService
	{
		private readonly IFieldService _fieldService;

		private readonly ContentService _contentService;

		public DefinitionEditorService(IServiceFactory serviceFactory, IFieldService fieldService)
		{
			_fieldService = fieldService;

			_contentService = serviceFactory.GetContentService();
		}


		public string PathSeparator { get; set; }

		public const string VirtualFieldPrefix = "V";

		/// <summary>
		///     возвращает Content или Field из rootContent по пути
		/// </summary>
		/// <param name="rootContent"></param>
		/// <param name="path">разделенные Separator идшники контентов и полей</param>
		/// <param name="notFoundInDef">
		///     был ли в rootContent найден по этому пути объект, если нет, будет возвращен с дефолтными
		///     настройками
		/// </param>
		/// <returns></returns>
		public object GetObjectFromPath(Content rootContent, string path, out bool notFoundInDef)
		{
			var entityIds = path.Split(new[] {PathSeparator}, StringSplitOptions.RemoveEmptyEntries)
				.Select(x =>
				{
					int fieldId;

					if (int.TryParse(x, out fieldId))
						return (object) fieldId;
					else
						return x.Substring(VirtualFieldPrefix.Length);
				});

			return GetObjectFromDef(rootContent, out notFoundInDef, entityIds);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rootContent"></param>
		/// <param name="notFoundInDef"></param>
		/// <param name="entityIds">int c id поля для невиртуальных полей и string с именем поля для виртуальных</param>
		/// <returns></returns>
		private object GetObjectFromDef(Content rootContent, out bool notFoundInDef, IEnumerable<object> entityIds)
		{
			object[] entityIdsToSearch = entityIds
				.Skip(1) //корень ровно один поэтому первый компонент пути нам не нужен
				.ToArray();

			object currentPositionObj = rootContent;

			notFoundInDef = false;

			foreach (object entityId in entityIdsToSearch)
			{
				if (currentPositionObj is Content)
				{
					var currContent = (Content)currentPositionObj;

					if(entityId is int)
						currentPositionObj = currContent.Fields.SingleOrDefault(x => x.FieldId == (int)entityId);
					else
						currentPositionObj = currContent.Fields.SingleOrDefault(x => x.FieldName == (string)entityId && x is BaseVirtualField);

					if (currentPositionObj == null)
					{
						//дурацкая система что Dictionaries это поле с id=0
						if (entityId is int && (int)entityId == 0)
						{
							notFoundInDef = true;

							return new Dictionaries();
						}

						if (entityId is int)
						{
							int enitityIdInt = (int) entityId;

							var qpField = _fieldService.Read(enitityIdInt);

							if (qpField == null)
							{
								notFoundInDef = true;

								return null;
							}

							if (qpField.RelationType == RelationType.None && !qpField.IsClassifier)
							{
								currentPositionObj = new PlainField { FieldId = enitityIdInt, FieldName = qpField.Name };

								notFoundInDef = !currContent.LoadAllPlainFields;
							}
							else
							{
								using (_fieldService.CreateQpConnectionScope())
								{
									if (qpField.ContentId == currContent.ContentId)
										if (!qpField.IsClassifier)
											currentPositionObj = new EntityField
											{
												FieldId = enitityIdInt,
												FieldName = qpField.Name,
												CloningMode = CloningMode.UseExisting,
												Content =
													new Content {ContentId = qpField.RelateToContentId.Value, ContentName = qpField.RelatedToContent.Name}
											};
										else
										{
											currentPositionObj = new ExtensionField
											{
												FieldId = qpField.Id,
												FieldName = qpField.Name,
												CloningMode = CloningMode.UseExisting
											};

											var classifierContents = _fieldService.ListRelated(qpField.ContentId)
												.Where(x => x.Aggregated)
												.Select(x => x.Content);

											foreach (var classifierContent in classifierContents)
											{
												((ExtensionField) currentPositionObj).ContentMapping.Add(
													classifierContent.Id,
													new Content {ContentId = classifierContent.Id, ContentName = classifierContent.Name});
											}
										}
									else
									{
										currentPositionObj = new BackwardRelationField
										{
											FieldId = qpField.Id,
											FieldName = qpField.Name,
											Content = new Content {ContentId = qpField.ContentId, ContentName = qpField.Content.Name},
											CloningMode = CloningMode.UseExisting
										};
									}
								}

								notFoundInDef = !qpField.IsClassifier || !currContent.LoadAllPlainFields;
							}
						}
					}
				}
				else
				{
					if (entityId is int)
					{
						int enitityIdInt = (int) entityId;
						
						Content[] contentsInField = GetContentsFromField((Field) currentPositionObj);

						currentPositionObj = contentsInField.SingleOrDefault(x => x.ContentId == enitityIdInt);

						if (currentPositionObj == null)
						{
							currentPositionObj = new Content { ContentId = enitityIdInt, ContentName = _contentService.Read(enitityIdInt).Name };

							notFoundInDef = true;
						}
					}
				}
			}

			return currentPositionObj;
		}

		public Content[] GetContentsFromField(Field field)
		{
			if (field is ExtensionField)
				return ((ExtensionField)field).ContentMapping.Values.ToArray();

			if (field is BackwardRelationField)
				return new[] { ((BackwardRelationField)field).Content };

			if (field is EntityField)
				return new[] { ((EntityField)field).Content };

			if (field is Dictionaries && ((Dictionaries) field).ContentDictionaries != null)
				return ((Dictionaries) field).ContentDictionaries.Values.ToArray();

			return new Content[0];
		}


		public object GetParentObjectFromPath(Content rootContent, string path)
		{
			string[] pathStrings = path.Split(new[] { PathSeparator }, StringSplitOptions.RemoveEmptyEntries);

			IEnumerable<object> entityIds = pathStrings.Take(pathStrings.Length - 1)
				.Select(x =>
				{
					int fieldId;

					if (int.TryParse(x, out fieldId))
						return (object) fieldId;
					else
						return x.Substring(VirtualFieldPrefix.Length);
				});

			bool notFoundInDef;

			object result = GetObjectFromDef(rootContent, out notFoundInDef, entityIds);

			if (notFoundInDef)
				throw new Exception("Не найден элемент по пути " + path);

			return result;
		}

		public Field UpdateOrDeleteField(Content rootContent, Field field, string path, bool doDelete)
		{
			bool notFoundInDef;

			var fieldToSave = (Field)GetObjectFromPath(rootContent, path, out notFoundInDef);

			var parentContent = (Content)GetParentObjectFromPath(rootContent, path);

			using (_fieldService.CreateQpConnectionScope())
			{
				//нужно удалить из описания
				if (doDelete && !notFoundInDef)
				{
					parentContent.Fields.Remove(fieldToSave);

					if (parentContent.LoadAllPlainFields && (fieldToSave is PlainField || fieldToSave is ExtensionField))
					{
						parentContent.LoadAllPlainFields = false;

						var allPlainFields = _fieldService.List(parentContent.ContentId).Where(x => x.RelationType == RelationType.None);

						parentContent.Fields.AddRange(allPlainFields
							.Where(x => x.Id != fieldToSave.FieldId && parentContent.Fields.All(y => y.FieldId != x.Id))
							.Select(x => new PlainField { FieldId = x.Id, FieldName = x.Name })
							.ToArray());
					}
				}

				if (!doDelete)
				{
                    if (parentContent.Fields.All(x => x.FieldId != fieldToSave.FieldId
                        || (x is BaseVirtualField && fieldToSave is BaseVirtualField && x.FieldName != fieldToSave.FieldName)))
                    {
                        parentContent.Fields.Add(fieldToSave);
                    }

					fieldToSave.FieldName = field.FieldName;
                    fieldToSave.FieldTitle = field.FieldTitle;

                    fieldToSave.CustomProperties.Clear();

                    foreach (var customPropKv in field.CustomProperties)
                    {
                        fieldToSave.CustomProperties[customPropKv.Key] = customPropKv.Value;
                    }

                    if (field is Association assosiationDef)
					{
						var assosiationToSave = (Association)fieldToSave;

						assosiationToSave.CloningMode = assosiationDef.CloningMode;
						assosiationToSave.DeletingMode = assosiationDef.DeletingMode;
                        assosiationToSave.UpdatingMode = assosiationDef.UpdatingMode;

                        if (field is EntityField entityDef)
                        {
                            var entityToSave = (EntityField)fieldToSave;
                            entityToSave.PreloadingMode = entityDef.PreloadingMode;
                            entityToSave.RelationCondition = entityDef.RelationCondition;
                            entityToSave.ClonePrototypeCondition = entityDef.ClonePrototypeCondition;

                            if (field is BackwardRelationField backwardDef)
                            {
                                var backwardToSave = (BackwardRelationField)fieldToSave;
                                backwardToSave.DisplayName = backwardDef.DisplayName;
                            }
                        }
					}
					else if (field is Dictionaries dictionariesDef)
                    {
                        var dictionariasToSave = ((Dictionaries)fieldToSave);
                        dictionariasToSave.DefaultCachePeriod = dictionariesDef.DefaultCachePeriod;
                    }
					else if (field is VirtualField virtFieldDef)
					{
						var virtFieldToSave = (VirtualField)fieldToSave;
						virtFieldToSave.Path = virtFieldDef.Path;
						virtFieldToSave.ObjectToRemovePath = virtFieldDef.ObjectToRemovePath;
					}
				}

				return doDelete ? null : fieldToSave;
			}
		}

		public int[] GetAllContentIdsFromTree(Content rootContent)
		{
			var allContents = new HashSet<Content>();

			FillAllContentsFromTreeRecursive(rootContent, allContents);

			return allContents.Select(x => x.ContentId).Distinct().ToArray();
		}

		private void FillAllContentsFromTreeRecursive(Content currContent, HashSet<Content> processedContents)
		{
			if(processedContents.Contains(currContent))
				return;

			processedContents.Add(currContent);

			foreach (var content in currContent.Fields.SelectMany(GetContentsFromField))
				FillAllContentsFromTreeRecursive(content, processedContents);
		}
	}
}
