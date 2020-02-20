using System;
using System.Linq;
using QA.Core.Models.Configuration;
using QA.Core.Models.Processors;
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
            var articleData = DPathProcessor.VerifyAndParseExpression(path).ToList();
            hasFilter = articleData.Any(ad => ad.FiltersData.Any());
            parent = definition;

            Field currentField = null;
            foreach (var fieldName in articleData.Select(ad => ad.FieldName))
            {
                if (currentField != null)
                {
                    var currentEntityField = currentField as EntityField;
                    if (currentEntityField == null)
                    {
                        throw new Exception("Schema generator requires virtual field to relate to EntityField type or its descendant");
                    }

                    parent = currentEntityField.Content;
                }

                var nonVirtualFields = parent.Fields.Where(x => !(x is BaseVirtualField)).ToArray();
                if (nonVirtualFields.All(x => x.FieldName != fieldName) && parent.LoadAllPlainFields)
                {
                    var allPlainFields = _fieldService.List(parent.ContentId).Where(x => x.RelationType == RelationType.None);
                    currentField = new PlainField
                    {
                        FieldId = allPlainFields.Single(x => x.Name == fieldName).Id,
                        FieldName = fieldName
                    };
                }
                else
                {
                    currentField = nonVirtualFields.Single(x => x.FieldName == fieldName);
                }
            }

            return currentField;
        }
    }
}
