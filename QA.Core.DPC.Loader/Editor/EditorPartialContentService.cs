using QA.Core.Models.Configuration;
using QA.Core.Models.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace QA.Core.DPC.Loader.Editor
{
    /// <summary>
    /// Выделение частичного определения продукта для сохранения подграфа статей
    /// </summary>
    public class EditorPartialContentService
    {
        private class PartialContentContext
        {
            public ReferenceDictionary<Content, Content> ContentPrototypesByReference;

            public Dictionary<Content, Content> ContentPrototypesByValue;

            public ReferenceHashSet<Content> VisitedContents;

            public HashSet<string> SelectedFieldPaths;
        }

        private static readonly Regex PathRegex
            = new Regex("^(/[1-9][0-9]*:[1-9][0-9]*)*/[1-9][0-9]*$", RegexOptions.Compiled);

        /// <summary>
        /// Выделение частичного определения продукта для сохранения подграфа статей
        /// по переданным путям контентов в формате <c>"/contentId:fieldId/.../contentId"</c>
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        public Content GetPartialContent(Content rootContent, string[] contentPaths)
        {
            if (rootContent == null) throw new ArgumentNullException(nameof(rootContent));
            if (contentPaths == null) throw new ArgumentNullException(nameof(contentPaths));
            if (contentPaths.Length == 0) throw new ArgumentException("Paths should not be empty", nameof(contentPaths));

            for (int i = 0; i < contentPaths.Length; i++)
            {
                if (!PathRegex.IsMatch(contentPaths[i]))
                {
                    throw new InvalidOperationException($"Path [{i}] \"{contentPaths[i]}\" is invalid");
                }
            }

            var context = new PartialContentContext();

            // клонируем rootContent, потому что его описание будет изменено в RemoveNotSelectedFields
            rootContent = rootContent.DeepCopy();

            // находим дубликаты контентов и сохраняем в словаре
            context.ContentPrototypesByReference = new ReferenceDictionary<Content, Content>();
            context.ContentPrototypesByValue = new Dictionary<Content, Content>();
            FillContentPrototypesDictionary(rootContent, context);

            // заменяем дубликаты контентов на их прообразы
            context.VisitedContents = new ReferenceHashSet<Content>();
            DecuplicateContents(rootContent, context);

            // находим контент по пути
            Content foundContent = FindContentByPath(rootContent, contentPaths[0]);

            // удаляем все связи между контентами, кроме описанных в contentPaths
            context.VisitedContents = new ReferenceHashSet<Content>();
            context.SelectedFieldPaths = new HashSet<string>(contentPaths.Skip(1).Select(GetFieldPath));
            RemoveNotSelectedFields(rootContent, context, "");

            return foundContent;
        }

        private static string GetFieldPath(string fieldContentPath)
        {
            return fieldContentPath.Substring(0, fieldContentPath.LastIndexOf('/'));
        }

        private void FillContentPrototypesDictionary(Content content, PartialContentContext context)
        {
            if (context.ContentPrototypesByReference.ContainsKey(content))
            {
                return;
            }
            if (context.ContentPrototypesByValue.ContainsKey(content))
            {
                context.ContentPrototypesByReference[content] = context.ContentPrototypesByValue[content];
            }
            else
            {
                context.ContentPrototypesByValue[content] = content;
                context.ContentPrototypesByReference[content] = content;
            }

            foreach (Association field in content.Fields.OfType<Association>())
            {
                foreach (Content childContent in field.Contents)
                {
                    FillContentPrototypesDictionary(childContent, context);
                }
            }
        }

        private void DecuplicateContents(Content content, PartialContentContext context)
        {
            if (context.VisitedContents.Contains(content))
            {
                return;
            }

            context.VisitedContents.Add(content);

            foreach (Field field in content.Fields)
            {
                if (field is EntityField entityField)
                {
                    entityField.Content = context.ContentPrototypesByReference[entityField.Content];

                    DecuplicateContents(entityField.Content, context);
                }
                else if (field is ExtensionField extensionField)
                {
                    foreach (var pair in extensionField.ContentMapping.ToArray())
                    {
                        Content childContent = context.ContentPrototypesByReference[pair.Value];

                        extensionField.ContentMapping[pair.Key] = childContent;

                        DecuplicateContents(childContent, context);
                    }
                }
            }
        }

        /// <exception cref="InvalidOperationException" />
        private Content FindContentByPath(Content content, string contentPath)
        {
            var pathNotFound = new InvalidOperationException($"Content not found for path \"{contentPath}\"");

            string[] pathSegments = contentPath.Split(':');

            string[] rootContent = pathSegments[0].Split('/');
            int rootContentId = Int32.Parse(rootContent[1]);
            if (content.ContentId != rootContentId)
            {
                throw pathNotFound;
            }

            foreach (string pathSegment in pathSegments.Skip(1))
            {
                string[] fieldContent = pathSegment.Split('/');
                int fieldId = Int32.Parse(fieldContent[0]);
                int contentId = Int32.Parse(fieldContent[1]);

                Field field = content.Fields.FirstOrDefault(f => f.FieldId == fieldId);

                if (field is EntityField entityField)
                {
                    content = entityField.Content;

                    if (content.ContentId != contentId)
                    {
                        throw pathNotFound;
                    }
                }
                else if (field is ExtensionField extensionField)
                {
                    content = extensionField.ContentMapping.Values
                        .FirstOrDefault(c => c.ContentId == contentId);

                    if (content == null)
                    {
                        throw pathNotFound;
                    }
                }
                else
                {
                    throw pathNotFound;
                }
            }

            return content;
        }

        private void RemoveNotSelectedFields(Content content, PartialContentContext context, string fieldPath)
        {
            if (context.VisitedContents.Contains(content))
            {
                return;
            }

            context.VisitedContents.Add(content);

            string contentPath = fieldPath + $"/{content.ContentId}";

            foreach (Association field in content.Fields.OfType<Association>().ToArray())
            {
                fieldPath = contentPath + $":{field.FieldId}";

                if (!context.SelectedFieldPaths.Contains(fieldPath))
                {
                    content.Fields.Remove(field);
                }

                foreach (Content childContent in field.Contents)
                {
                    RemoveNotSelectedFields(childContent, context, fieldPath);
                }
            }
        }
    }
}
