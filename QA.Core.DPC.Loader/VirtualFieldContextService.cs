using System;
using System.Collections.Generic;
using QA.Core.Models.Configuration;

namespace QA.Core.DPC.Loader
{
    public class VirtualFieldContext
    {
        /// <summary>
        /// Значения вирутальных полей
        /// </summary>
        public Dictionary<Tuple<Content, string>, Field> VirtualFields
            = new Dictionary<Tuple<Content, string>, Field>();

        /// <summary>
        /// Cписок виртуальных полей которые надо игнорировать при создании схемы
        /// </summary>
        public List<Tuple<Content, Field>> IgnoredFields = new List<Tuple<Content, Field>>();
    }

    public class VirtualFieldContextService
    {
        private readonly VirtualFieldPathEvaluator _virtualFieldPathEvaluator;

        public VirtualFieldContextService(VirtualFieldPathEvaluator virtualFieldPathEvaluator)
        {
            _virtualFieldPathEvaluator = virtualFieldPathEvaluator;
        }

        /// <summary>
        /// Рекурсивно обходит <see cref="Content"/> и заполняет
        /// <see cref="VirtualFieldContext.IgnoredFields"/> - список полей которые надо игнорировать при создании схемы
        /// и <see cref="VirtualFieldContext.VirtualFields"/> - значения вирутальных полей
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        public VirtualFieldContext GetVirtualFieldContext(Content content)
        {
            var context = new VirtualFieldContext();

            FillVirtualFieldsInfo(content, context, new HashSet<Content>());

            return context;
        }

        private void FillVirtualFieldsInfo(
            Content content, VirtualFieldContext context, HashSet<Content> visitedContents)
        {
            if (visitedContents.Contains(content))
            {
                return;
            }
            visitedContents.Add(content);

            foreach (Field field in content.Fields)
            {
                if (field is BaseVirtualField baseField)
                {
                    ProcessVirtualField(baseField, content, context);
                }
                else if (field is EntityField entityField)
                {
                    FillVirtualFieldsInfo(entityField.Content, context, visitedContents);
                }
                else if (field is ExtensionField extensionField)
                {
                    foreach (Content extContent in extensionField.ContentMapping.Values)
                    {
                        FillVirtualFieldsInfo(extContent, context, visitedContents);
                    }
                }
            }
        }
        
        private void ProcessVirtualField(
            BaseVirtualField baseVirtualField, Content definition, VirtualFieldContext context)
        {
            if (baseVirtualField is VirtualMultiEntityField virtualMultiEntityField)
            {
                string path = virtualMultiEntityField.Path;

                var virtualFieldKey = Tuple.Create(definition, path);

                if (context.VirtualFields.ContainsKey(virtualFieldKey))
                {
                    return;
                }

                Field foundField = _virtualFieldPathEvaluator
                    .GetFieldByPath(path, definition, out bool hasFilter, out Content parent);

                if (!hasFilter)
                {
                    context.IgnoredFields.Add(Tuple.Create(parent, foundField));
                }

                context.VirtualFields[virtualFieldKey] = foundField;

                if (foundField is EntityField foundEntityField)
                {
                    foreach (BaseVirtualField childField in virtualMultiEntityField.Fields)
                    {
                        ProcessVirtualField(childField, foundEntityField.Content, context);
                    }
                }
                else
                {
                    throw new InvalidOperationException(
                        "В Path VirtualMultiEntityField должны быть только поля EntityField или наследники");
                }
            }
            else if (baseVirtualField is VirtualEntityField virtualEntityField)
            {
                foreach (BaseVirtualField childField in virtualEntityField.Fields)
                {
                    ProcessVirtualField(childField, definition, context);
                }
            }
            else if (baseVirtualField is VirtualField virtualField)
            {
                string path = virtualField.Path;

                var virtualFieldKey = Tuple.Create(definition, path);

                if (context.VirtualFields.ContainsKey(virtualFieldKey))
                {
                    return;
                }

                Field fieldToMove = _virtualFieldPathEvaluator
                    .GetFieldByPath(path, definition, out bool hasFilter, out Content parent);

                if (!hasFilter)
                {
                    context.IgnoredFields.Add(Tuple.Create(parent, fieldToMove));
                }

                context.VirtualFields[virtualFieldKey] = fieldToMove;
            }
        }
    }
}
