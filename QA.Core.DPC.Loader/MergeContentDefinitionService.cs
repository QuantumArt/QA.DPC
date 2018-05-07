using QA.Core.Models.Configuration;
using QA.Core.Models.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QA.Core.DPC.Loader
{
    public class MergeContentDefinitionService
    {
        private readonly VirtualFieldContextService _virtualFieldContextService;

        public MergeContentDefinitionService(VirtualFieldContextService virtualFieldContextService)
        {
            _virtualFieldContextService = virtualFieldContextService;
        }

        private class Context
        {
            /// <summary>
            /// Множество посещенных контентов
            /// </summary>
            public ReferenceHashSet<Content> VisitedContents = new ReferenceHashSet<Content>();

            /// <summary>
            /// Группировка контентов по <see cref="Content.ContentId"/>
            /// </summary>
            public Dictionary<int, List<Content>> ContentsById = new Dictionary<int, List<Content>>();

            /// <summary>
            /// Cписок виртуальных полей которые надо игнорировать при создании схемы
            /// </summary>
            public ReferenceDictionary<Content, ReferenceHashSet<Field>> IgnoredFieldsByContent
                = new ReferenceDictionary<Content, ReferenceHashSet<Field>>();

            public bool IsFieldIgnored(Content content, Field field)
            {
                return IgnoredFieldsByContent.TryGetValue(content, out ReferenceHashSet<Field> ignoredFields)
                    && ignoredFields.Contains(field);
            }
        }

        /// <summary>
        /// Рекурсивно обходим дерево контентов. Для всех контентов, <see cref="Content.ContentId"/>
        /// которых встретился больше одного раза объединяем наборы полей. При этом такие свойства,
        /// как <see cref="Association.CloningMode"/> остаются своими для каждого поля.
        /// </summary>
        public void MergeDuplicatedContents(Content rootContent)
        {
            VirtualFieldContext virtualFieldContext = _virtualFieldContextService
                .GetVirtualFieldContext(rootContent);

            var context = new Context();

            if (virtualFieldContext.IgnoredFields.Count > 0)
            {
                FillIgnoredFieldsByContent(rootContent, context, virtualFieldContext.IgnoredFields);

                // очищаем множество посещенных контентов
                context.VisitedContents = new ReferenceHashSet<Content>();
            }

            VisitContent(rootContent, context);
        }

        /// <summary>
        /// Метод <see cref="VisitContent"/> изменяет посещенные контенты. Из-за этого мы
        /// не сможем использовать <see cref="Content.RecursiveEquals"/> при сравнении контентов
        /// в <see cref="Tuple{Content, Field}"/>. Поэтому мы обходим граф контентов
        /// и заполняем ссылочный словарь вида (контент => игнорируемые поля)
        /// </summary>
        private void FillIgnoredFieldsByContent(
            Content content, Context context, List<Tuple<Content, Field>> ignoredFields)
        {
            if (context.VisitedContents.Contains(content))
            {
                return;
            }

            context.VisitedContents.Add(content);

            foreach (Field field in content.Fields)
            {
                if (ignoredFields.Contains(Tuple.Create(content, field)))
                {
                    if (context.IgnoredFieldsByContent.TryGetValue(content, out ReferenceHashSet<Field> fields))
                    {
                        fields.Add(field);
                    }
                    else
                    {
                        context.IgnoredFieldsByContent[content] = new ReferenceHashSet<Field> { field };
                    }
                }
                if (field is Association association)
                {
                    foreach (Content childContent in association.Contents)
                    {
                        FillIgnoredFieldsByContent(childContent, context, ignoredFields);
                    }
                }
            }
        }

        private void VisitContent(Content content, Context context)
        {
            if (context.VisitedContents.Contains(content))
            {
                return;
            }

            context.VisitedContents.Add(content);

            if (context.ContentsById.TryGetValue(content.ContentId, out List<Content> existingContents))
            {
                foreach (Content existingContent in existingContents)
                {
                    MergeFields(existingContent, content, context);
                }

                existingContents.Add(content);
            }
            else
            {
                context.ContentsById[content.ContentId] = new List<Content> { content };
            }

            // При итерации по коллекции content.Fields мы можем прийти в тот же контент
            // и изменить саму коллекцию. Поэтому — .ToArray()
            foreach (Association association in content.Fields.OfType<Association>().ToArray())
            {
                if (!context.IsFieldIgnored(content, association))
                {
                    foreach (Content childContent in association.Contents)
                    {
                        VisitContent(childContent, context);
                    }
                }
            }
        }

        /// <summary>
        /// Объединяем коллекции <see cref="Content.Fields"/> и сохраняем в обоих контентах
        /// </summary>
        private void MergeFields(Content leftContent, Content rightContent, Context context)
        {
            if (leftContent.LoadAllPlainFields || rightContent.LoadAllPlainFields)
            {
                leftContent.LoadAllPlainFields = rightContent.LoadAllPlainFields;
            }

            foreach (Field leftField in leftContent.Fields)
            {
                if (!(leftField is Dictionaries
                    || leftField is BaseVirtualField
                    || rightContent.Fields.Any(f => f.FieldId == leftField.FieldId)
                    || context.IsFieldIgnored(leftContent, leftField)))
                {
                    rightContent.Fields.Add(leftField);
                }
            }

            foreach (Field righrField in rightContent.Fields)
            {
                if (!(righrField is Dictionaries
                    || righrField is BaseVirtualField
                    || leftContent.Fields.Any(f => f.FieldId == righrField.FieldId)
                    || context.IsFieldIgnored(rightContent, righrField)))
                {
                    leftContent.Fields.Add(righrField);
                }
            }
        }
    }
}