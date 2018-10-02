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
    }
}
