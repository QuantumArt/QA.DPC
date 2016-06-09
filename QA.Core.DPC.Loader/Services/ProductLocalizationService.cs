using QA.ProductCatalog.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Models.Entities;
using System.Globalization;
using QA.Core.Models.Extensions;

namespace QA.Core.DPC.Loader.Services
{

    class ProductLocalizationService : IProductLocalizationService
    {
        private readonly ILocalizationSettingsService _settingsService;

        public ProductLocalizationService(ILocalizationSettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        #region IProductLocalizationService implementation
        public LocalizedArticle Localize(Article product, CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            var localizationMap = _settingsService.GetSettings(0);

            var articleMap = CloneArticles(product);

            foreach(var item in articleMap)
            {
                var fieldGroups = GetLocalizedFields(item.Key, localizationMap);

                foreach (var fields in fieldGroups)
                {
                    LocalizedField field = null;
                    var currentCulture = culture;

                    do
                    {
                        field = fields.FirstOrDefault(f => f.Culture.Equals(currentCulture));

                        if (currentCulture.Equals(CultureInfo.InvariantCulture))
                        {
                            break;
                        }
                        else
                        {
                            currentCulture = currentCulture.Parent;
                        }
                    }
                    while (field == null);


                    if (field == null)
                    {
                        throw new Exception($"Для языка {culture.DisplayName} не задано поле {fields.Key}");
                    }
                    else
                    {
                        var newField = CloneField(field.Field, field.InvariantFieldName, articleMap);
                        item.Value.Fields[newField.FieldName] = newField;
                    }
                }
            }

            return new LocalizedArticle(articleMap[product], culture);
        }

        public Article MergeLocalizations(LocalizedArticle[] localizations)
        {
            throw new NotImplementedException();
        }

        public Article MergeLocalizations(Article product, LocalizedArticle[] localizations)
        {
            throw new NotImplementedException();
        }

        public LocalizedArticle[] SplitLocalizations(Article product)
        {
            throw new NotImplementedException();
        }

        public LocalizedArticle[] SplitLocalizations(Article product, CultureInfo[] cultures)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Private methods
        private Dictionary<Article, Article> CloneArticles(Article article)
        {
            return article
                .GetAllArticles()
                .ToDictionary(
                    a => a,
                    a => CloneArticle(a)
                );            
        }

        private Article CloneArticle(Article article)
        {
            return new Article
            {
                Id = article.Id,
                Archived = article.Archived,
                ContentId = article.ContentId,
                ContentName = article.ContentName,
                ContentDisplayName = article.ContentDisplayName,
                Created = article.Created,
                IsPublished = article.IsPublished,
                HasVirtualFields = article.HasVirtualFields,
                Modified = article.Modified,
                PublishingMode = article.PublishingMode,
                Splitted = article.Splitted,
                Status = article.Status,
                Visible = article.Visible,
            };
        }
        private ArticleField CloneField(ArticleField field, string fieldName, Dictionary<Article, Article> articleMap)
        {
            var pField = field as PlainArticleField;
            var sField = field as SingleArticleField;

            if (pField != null)
            {
                return new PlainArticleField
                {
                    FieldName = fieldName,
                    ContentId = pField.ContentId,
                    FieldId = pField.FieldId,
                    CustomProperties = pField.CustomProperties,
                    FieldDisplayName = pField.FieldDisplayName,
                    NativeValue = pField.NativeValue,
                    Value = pField.Value,
                    PlainFieldType = pField.PlainFieldType
                };
            }
            else if (sField != null)
            {
                return new SingleArticleField
                {
                    FieldName = fieldName,
                    ContentId = sField.ContentId,
                    FieldId = sField.ContentId,
                    Aggregated = sField.Aggregated,
                    CustomProperties = sField.CustomProperties,
                    FieldDisplayName = sField.FieldDisplayName,
                    SubContentId = sField.SubContentId,
                    Item = articleMap[sField.Item]
                };
            }
            else
            {
                return null;
            }
        }
        public IEnumerable<IGrouping<string, LocalizedField>> GetLocalizedFields(Article article, Dictionary<string, CultureInfo> localizationMap)
        {
            var fields = from field in article
                         let suffix = localizationMap.Keys
                             .Where(l => field.FieldName.EndsWith(l))
                             .OrderByDescending(l => l.Length)
                             .FirstOrDefault()
                         let fieldName = field.FieldName
                         select new LocalizedField
                         {
                             Field = field,
                             InvariantFieldName = suffix == null ?
                                  fieldName :
                                  fieldName.Remove(fieldName.Length - suffix.Length),
                             Culture = suffix == null ?
                                  CultureInfo.InvariantCulture :
                                  localizationMap[suffix]
                         };

            var gr = fields.GroupBy(f => f.InvariantFieldName);

            var x = gr
                .Where(
                    g => g.Select(f => f.Field.GetType())
                    .Distinct()
                    .Count() > 1
                    ||
                    g.Select(f => f.Field)
                    .OfType<PlainArticleField>()
                    .Select(f => f.PlainFieldType)
                    .Distinct()
                    .Count() > 1
                    )
                .Select(g => g.Key)
                .ToArray();

            if (x.Any())
            {
                throw new Exception($"Поля {string.Join(", ", x)} имеют разный тип в локализованных версиях");
            }

            return gr;
        }
        #endregion
    }

    public class LocalizedField
    {
        public ArticleField Field { get; set; }
        public string InvariantFieldName { get; set; }
        public CultureInfo Culture { get; set; }

    }
}
