using System;
using System.Linq;
using System.Text.RegularExpressions;
using QA.Core.Models.Configuration;
using QA.Core.Models.UI;
using QA.Core.ProductCatalog.Actions.Services;
using Quantumart.QP8.BLL;
using Content = QA.Core.Models.Configuration.Content;
using Field = QA.Core.Models.Configuration.Field;

namespace QA.Core.DPC.Loader
{
	public class VirtualFieldPathEvaluator
	{
		private readonly IFieldService _fieldService;
		
		public VirtualFieldPathEvaluator(IFieldService fieldService)
		{
			_fieldService = fieldService;
		}

		public Field GetFieldByPath(string path, Content definition, out bool hasFilter, out Content parent)
		{
			var pathMatch = FilterableBindingValueProvider.VirtualFieldPathRegex.Match(path);

			if(!pathMatch.Success)
				throw new ArgumentException("Некорректный формат пути: " + path);

			hasFilter = pathMatch.Groups[FilterableBindingValueProvider.FILTER_VALUE_REGEX_GROUP_NAME].Success;

			parent = definition;

			var fieldNames = pathMatch.Groups[FilterableBindingValueProvider.FIELD_REGEX_GROUP_NAME].Captures.Cast<Capture>().Select(x => x.Value);

			Field currentField = null;

			foreach (string fieldName in fieldNames)
			{
				if (currentField != null)
				{
					var currentEntityField = currentField as EntityField;

					if (currentEntityField == null)
						throw new Exception("Генерация схемы требует что бы виртуальные поля ссылались на тип EntityField или наследник");

					parent = currentEntityField.Content;
				}

				var nonVirtualFields = parent.Fields.Where(x => !(x is BaseVirtualField)).ToArray();

				if (nonVirtualFields.All(x => x.FieldName != fieldName) && parent.LoadAllPlainFields)
				{
					var allPlainFields = _fieldService.List(parent.ContentId).Where(x=>x.RelationType==RelationType.None);

					currentField = new PlainField {FieldId = allPlainFields.Single(x => x.Name == fieldName).Id, FieldName = fieldName};
				}
				else
					currentField = nonVirtualFields.Single(x => x.FieldName == fieldName);
			}

			return currentField;
		}

		
	}
}
