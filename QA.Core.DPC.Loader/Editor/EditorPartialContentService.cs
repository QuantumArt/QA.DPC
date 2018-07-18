using QA.Core.Models.Configuration;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Quantumart.QP8.BLL.Services.API;

namespace QA.Core.DPC.Loader.Editor
{
    /// <summary>
    /// Выделение частичного определения продукта для сохранения подграфа статей
    /// </summary>
    public class EditorPartialContentService
    {
        private readonly ContentService _contentService;

        public EditorPartialContentService(ContentService contentService)
        {
            _contentService = contentService;
        }

        private static readonly Regex PathRegex = new Regex("^(/?|(/[A-Za-z]+)+)$", RegexOptions.Compiled);

        /// <summary>
        /// Найти контент по его пути <paramref name="contentPath"/>
        /// от корневого контента <paramref name="rootContent"/>
        /// в формате <c>"/FieldName/.../ExtensionContentName/.../FieldName"</c>
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        public Content FindContentByPath(Content rootContent, string contentPath, bool withDictionaries = false)
        {
            if (rootContent == null) throw new ArgumentNullException(nameof(rootContent));
            if (contentPath == null) throw new ArgumentNullException(nameof(contentPath));
            if (!PathRegex.IsMatch(contentPath))
            {
                throw new ArgumentException($"Content path \"{contentPath}\" is invalid", nameof(contentPath));
            }

            _contentService.LoadStructureCache();

            Content content = rootContent;

            string[] pathSegments = contentPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < pathSegments.Length; i++)
            {
                Field field = content.Fields.FirstOrDefault(f => f.FieldName == pathSegments[i]);

                if (field is EntityField entityField)
                {
                    content = entityField.Content;
                    continue;
                }
                if (field is ExtensionField extensionField && i + 1 < pathSegments.Length)
                {
                    // выбираем контент-расширение и пропускаем шаг
                    content = extensionField.ContentMapping.Values
                        .FirstOrDefault(c => _contentService.Read(c.ContentId).NetName == pathSegments[i + 1]);

                    if (content != null)
                    {
                        i++;
                        continue;
                    }
                }
                throw new InvalidOperationException($"Content not found for path \"{contentPath}\""); ;
            }

            if (withDictionaries && content != rootContent)
            {
                content = content.ShallowCopy();
                content.Fields.AddRange(rootContent.Fields.OfType<Dictionaries>());
            }

            return content;
        }

        /// <summary>
        /// Выделение частичного определения продукта для сохранения подграфа статей
        /// начиная с корневого контента, описанного путём <paramref name="contentPath"/>
        /// в формате <c>"/FieldName/.../ExtensionContentName/.../FieldName"</c>,
        /// и поддеревом выбора частичного продукта <paramref name="relationSelection"/>.
        /// Если <code>relationSelection == null</code> — то выбирается весь подграф продукта целиком.
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        public Content GetPartialContent(
            Content rootContent, string contentPath, RelationSelection relationSelection)
        {
            if (rootContent == null) throw new ArgumentNullException(nameof(rootContent));
            if (contentPath == null) throw new ArgumentNullException(nameof(contentPath));

            _contentService.LoadStructureCache();

            // находим контент по пути contentPath
            Content foundContent = FindContentByPath(rootContent, contentPath, withDictionaries: true);

            if (relationSelection == null)
            {
                return foundContent;
            }
            
            // выбираем отмеченные связи, удаляя при этом остальные
            Content partialContent = WithSelectedRelations(foundContent, relationSelection);
            
            return partialContent;
        }

        /// <exception cref="InvalidOperationException" />
        private Content WithSelectedRelations(Content content, RelationSelection contentSelection)
        {
            content = content.ShallowCopy();
            
            foreach (Association field in content.Fields.OfType<Association>().ToArray())
            {
                content.Fields.Remove(field);

                var newField = (Association)field.ShallowCopy();
                
                if (contentSelection.TryGetValue(newField.FieldName, out RelationSelection fieldSelection))
                {
                    if (newField is EntityField entityField)
                    {
                        entityField.Content = fieldSelection == null
                            ? WithoutAllRelations(entityField.Content)
                            : WithSelectedRelations(entityField.Content, fieldSelection);
                    }
                    else if (newField is ExtensionField extField)
                    {
                        RemoveNotSelectedRelationsFromExtension(extField, fieldSelection);
                    }
                    content.Fields.Add(newField);
                }
                else if (newField is ExtensionField extField)
                {
                    foreach (Content extContent in extField.ContentMapping.Values.ToArray())
                    {
                        extField.ContentMapping[extContent.ContentId] = WithoutAllRelations(extContent);
                    }
                    content.Fields.Add(newField);
                }
            }

            return content;
        }
        
        private Content WithoutAllRelations(Content content)
        {
            content = content.ShallowCopy();

            foreach (Association field in content.Fields.OfType<Association>().ToArray())
            {
                content.Fields.Remove(field);

                var newField = (Association)field.ShallowCopy();

                if (newField is ExtensionField extField)
                {
                    foreach (Content extContent in extField.ContentMapping.Values.ToArray())
                    {
                        extField.ContentMapping[extContent.ContentId] = WithoutAllRelations(extContent);
                    }
                    content.Fields.Add(newField);
                }
            }
            return content;
        }
        
        private void RemoveNotSelectedRelationsFromExtension(
            ExtensionField extField, RelationSelection fieldSelection)
        {
            if (fieldSelection == null)
            {
                throw new InvalidOperationException(
                    $"Selection for ExtensionField '{extField.FieldName}' should not be empty");
            }
            foreach (Content extContent in extField.ContentMapping.Values.ToArray())
            {
                string contentName = _contentService.Read(extContent.ContentId).NetName;

                if (fieldSelection.TryGetValue(
                    contentName, out RelationSelection extContentSelection))
                {
                    if (extContentSelection == null)
                    {
                        throw new InvalidOperationException(
                            $@"Selection for ExtensionField Content '{
                                extField.FieldName}.{contentName}' should not be empty");
                    }
                    extField.ContentMapping[extContent.ContentId] =
                        WithSelectedRelations(extContent, extContentSelection);
                }
                else
                {
                    extField.ContentMapping[extContent.ContentId] = WithoutAllRelations(extContent);
                }
            }
        }
    }
}
