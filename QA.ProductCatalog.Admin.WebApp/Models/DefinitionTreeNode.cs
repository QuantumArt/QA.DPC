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

		public bool NotInDefinition { get; set; }

		public bool MissingInQp = false;

		public bool IsDictionaries = false;

		public const string Separator = "/";

		public DefinitionTreeNode(Content content, string parentPath, string ownPath, bool isFromDictionaries, bool notInDefinition, ContentService contentService)
		{
			Id = ownPath ?? parentPath + Separator + content.ContentId;

			text = string.IsNullOrEmpty(content.ContentName) ? contentService.Read(content.ContentId).Name : content.ContentName;

			expanded = hasChildren = !isFromDictionaries && !notInDefinition;

			imageUrl = "Content/img/icons/content.gif";

			NotInDefinition = notInDefinition;
		}

		public DefinitionTreeNode(Quantumart.QP8.BLL.Content content, string parentPath)
		{
			Id = parentPath + Separator + content.Id;

			text = content.Name;

			expanded = false;

			hasChildren = false;

			imageUrl = "Content/img/icons/content.gif";

			NotInDefinition = true;
		}

		internal const string VirtualFieldPrefix = "V";

		public DefinitionTreeNode(Field fieldFromDef, string parentPath, string ownPath, bool missingInQp, bool notInDefinition)
		{
			Id = ownPath ?? parentPath + Separator + (fieldFromDef is BaseVirtualField ? VirtualFieldPrefix + fieldFromDef.FieldName : fieldFromDef.FieldId.ToString());

			text = fieldFromDef.FieldName;

			if (fieldFromDef is BackwardRelationField)
			{
				var backwardField = (BackwardRelationField) fieldFromDef;

				if (backwardField.Content != null)
					text += " из " + (backwardField.Content.ContentName ?? "контента " + backwardField.Content.ContentId);
			}

			expanded = false;

			hasChildren = !(fieldFromDef is PlainField) && !notInDefinition && !(fieldFromDef is VirtualField);

			if (fieldFromDef is Association)
				imageUrl = "Content/img/icons/relation.gif";
			else if(fieldFromDef is BaseVirtualField)
				imageUrl = "Content/img/icons/virtualField.gif";

			MissingInQp = missingInQp;

			NotInDefinition = notInDefinition;

			IsDictionaries = fieldFromDef is Dictionaries;
		}

		public DefinitionTreeNode(Quantumart.QP8.BLL.Field fieldNotInDef, string parentPath, string ownPath, bool notInDefinition, bool isFromRelatedContent, IFieldService fieldService)
		{
			Id = ownPath ?? parentPath + Separator + fieldNotInDef.Id;

			text = fieldNotInDef.Name;

			using (fieldService.CreateQpConnectionScope())
			{
				if (isFromRelatedContent)
					if (!fieldNotInDef.Aggregated)
						text += " из контента " + fieldNotInDef.Content.Name;
			}

			expanded = false;

			//ноды которые не включены в описание не надо позволять раскрывать пока их не включат
			hasChildren = false;

			NotInDefinition = notInDefinition;

			MissingInQp = false;

			if (fieldNotInDef.RelationType != RelationType.None || fieldNotInDef.IsClassifier)
				imageUrl = "Content/img/icons/relation.gif";
		}

		

		private static DefinitionTreeNode[] GetContentFields(Content content, IFieldService fieldService, string parentPath, bool isRootContent)
		{
			var qpFields = fieldService.List(content.ContentId).Concat(fieldService.ListRelated(content.ContentId)).ToArray();

			var fieldsNotInDef = qpFields.Where(x => content.Fields.All(y => y.FieldId != x.Id) && (!x.Aggregated || x.ContentId == content.ContentId));

			var contentFields = content.Fields
				.Select(x => new DefinitionTreeNode(x, parentPath, null, qpFields.All(y => y.Id != x.FieldId) && !(x is Dictionaries) && !(x is BaseVirtualField), false));

			if (isRootContent && !content.Fields.Any(x => x is Dictionaries))
				contentFields = contentFields.Concat(new[] {new DefinitionTreeNode(new Dictionaries(), parentPath, null, false, true)});

			return contentFields
				.Concat(fieldsNotInDef.Select(x => new DefinitionTreeNode(x, parentPath, null, x.RelationType != RelationType.None || !content.LoadAllPlainFields, x.ContentId != content.ContentId, fieldService)))
				.ToArray();
		}


		public static DefinitionTreeNode[] GetObjectsFromPath(Content rootContent, string path, IFieldService fieldService, DefinitionEditorService definitionEditorService, ContentService contentService)
		{
			//запрос корня
			if (string.IsNullOrEmpty(path))
				return new[] {new DefinitionTreeNode(rootContent, string.Empty, null, false, false, contentService)};
			
			bool notFoundInDef;

			object foundObject = definitionEditorService.GetObjectFromPath(rootContent, path, out notFoundInDef);

			if (foundObject is Content)
				return GetContentFields((Content) foundObject, fieldService, path, foundObject == rootContent);

			var contentsFromDef = definitionEditorService.GetContentsFromField((Field) foundObject);

			var nodesFromContentsInDef = contentsFromDef
				.Select(x => new DefinitionTreeNode(x, path, null, foundObject is Dictionaries, false, contentService))
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