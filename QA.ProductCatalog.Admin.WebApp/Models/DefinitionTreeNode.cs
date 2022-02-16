using System;
using System.Linq;
using QA.Core.DPC.Loader.Services;
using QA.Core.Models.Configuration;
using QA.Core.ProductCatalog.Actions.Services;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API;
using Content = QA.Core.Models.Configuration.Content;
using Field = QA.Core.Models.Configuration.Field;

namespace QA.ProductCatalog.Admin.WebApp.Models
{
	public class DefinitionTreeNode : KendoTreeNode
	{
		public string Id { get; set; }

		public bool InDefinition { get; set; }

		public bool MissingInQp = false;

		public bool IsDictionaries = false;

		public const string Separator = "/";

		public string IconName = null;

		public DefinitionTreeNode(Content content, ContentService contentService, string parentPath = "", string ownPath = null, bool isFromDictionaries = false, bool isExtensionField = false, bool inDefinition = false)
		{
			Id = ownPath ?? parentPath + Separator + content.ContentId;

			text = string.IsNullOrEmpty(content.ContentName) ? contentService.Read(content.ContentId).Name : content.ContentName;

			hasChildren = !isFromDictionaries && inDefinition;

            expanded = !isFromDictionaries && inDefinition && !isExtensionField;

			imageUrl = "images/icons/content.gif";

			IconName = "document";

			InDefinition = inDefinition;
		}

		public DefinitionTreeNode(Quantumart.QP8.BLL.Content content, string parentPath)
		{
			Id = parentPath + Separator + content.Id;

			text = content.Name;

			expanded = false;

			hasChildren = false;

			imageUrl = "images/icons/content.gif";

			InDefinition = false;

			IconName = "document";
		}

		internal const string VirtualFieldPrefix = "V";

		public DefinitionTreeNode(Field fieldFromDef, string parentPath = "", string ownPath = null, bool missingInQp = false, bool inDefinition = false)
		{
			Id = ownPath ?? parentPath + Separator + (fieldFromDef is BaseVirtualField ? VirtualFieldPrefix + fieldFromDef.FieldName : fieldFromDef.FieldId.ToString());

			text = fieldFromDef.FieldName;

            if (fieldFromDef is BackwardRelationField backwardField)
            {
                if (backwardField.Content != null)
                    text += " из " + (backwardField.Content.ContentName ?? "контента " + backwardField.Content.ContentId);
            }

            expanded = false;

			hasChildren = !(fieldFromDef is PlainField) && inDefinition && !(fieldFromDef is VirtualField);

			if (fieldFromDef is Association) {
				imageUrl = "images/icons/relation.gif";
				IconName = "link";
			}
			else if(fieldFromDef is BaseVirtualField) {
				imageUrl = "images/icons/virtualField.gif";
				IconName = "cube";
			}

			MissingInQp = missingInQp;

			InDefinition = inDefinition;

			IsDictionaries = fieldFromDef is Dictionaries;
		}

		public DefinitionTreeNode(Quantumart.QP8.BLL.Field field, IFieldService fieldService, string parentPath = "", string ownPath = null, bool inDefinition = false, bool isFromRelatedContent = false)
		{
			Id = ownPath ?? parentPath + Separator + field.Id;

			text = field.Name;

			using (fieldService.CreateQpConnectionScope())
			{
				if (isFromRelatedContent)
					if (!field.Aggregated)
						text += " из контента " + field.Content.Name;
			}

			expanded = false;

			//ноды которые не включены в описание не надо позволять раскрывать пока их не включат
			hasChildren = false;

			InDefinition = inDefinition;

			MissingInQp = false;

			if (field.RelationType != RelationType.None || field.IsClassifier) {
				imageUrl = "images/icons/relation.gif";
				IconName = "link";
			}
		}

		private static DefinitionTreeNode[] GetContentFields(Content content, IFieldService fieldService, string parentPath, bool isRootContent)
		{
			var qpFields = fieldService.List(content.ContentId).Concat(fieldService.ListRelated(content.ContentId)).ToArray();

			var fieldsNotInDef = qpFields.Where(x => content.Fields.All(y => y.FieldId != x.Id) && (!x.Aggregated || x.ContentId == content.ContentId));

            var contentFields = content.Fields
                .Select(x => new DefinitionTreeNode(
                        x,
                        parentPath,
                        missingInQp: qpFields.All(y => y.Id != x.FieldId) && !(x is Dictionaries) &&
                                     !(x is BaseVirtualField),
                        inDefinition: true
                    )
                );

			if (isRootContent && !content.Fields.Any(x => x is Dictionaries))
                contentFields = contentFields.Concat(new[]
                {
                    new DefinitionTreeNode(new Dictionaries(), parentPath)
                });

            return contentFields
                .Concat(fieldsNotInDef.Select(x => new DefinitionTreeNode(
                    x, fieldService, parentPath,
                    inDefinition: x.RelationType == RelationType.None && content.LoadAllPlainFields,
                    isFromRelatedContent: x.ContentId != content.ContentId
                )))
                .ToArray();
		}


		public static DefinitionTreeNode[] GetObjectsFromPath(Content rootContent, string path, IFieldService fieldService, DefinitionEditorService definitionEditorService, ContentService contentService)
		{
			//запрос корня
            if (string.IsNullOrEmpty(path))
                return new[]
                {
                    new DefinitionTreeNode(rootContent, contentService, inDefinition: true)
                };
			
			object foundObject = definitionEditorService.GetObjectFromPath(rootContent, path, out _);

			if (foundObject is Content)
				return GetContentFields((Content) foundObject, fieldService, path, foundObject == rootContent);

			var contentsFromDef = definitionEditorService.GetContentsFromField((Field) foundObject);

            var nodesFromContentsInDef = contentsFromDef
                .Select(x => new DefinitionTreeNode(
                    x, contentService, path,
                    isFromDictionaries: foundObject is Dictionaries,
                    isExtensionField: foundObject is ExtensionField,
                    inDefinition: true
                ))
                .ToArray();

			if (foundObject is ExtensionField)
			{
				var allExtensions = fieldService.ListRelated(fieldService.Read(((Field) foundObject).FieldId).ContentId)
					.Where(x => x.Aggregated)
					.Select(x => x.Content)
					.ToArray();

				return nodesFromContentsInDef
					.Concat(allExtensions
						.Where(x => contentsFromDef.All(y => y.ContentId != x.Id))
						.Select(x => new DefinitionTreeNode(x, path)))
					.ToArray();
			}
			else if (foundObject is Dictionaries)
			{
				int[] allContentIds = definitionEditorService.GetAllContentIdsFromTree(rootContent);

				return nodesFromContentsInDef
					.Concat(allContentIds
						.Where(x => contentsFromDef.All(y => y.ContentId != x))
						.Select(x => new DefinitionTreeNode(contentService.Read(x), path))
						.OrderBy(x=> x.text))
					.ToArray();
			}else
				return nodesFromContentsInDef;
		}

		

		

		
		
	}
}