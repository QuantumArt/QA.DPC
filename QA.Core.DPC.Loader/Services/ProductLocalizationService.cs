using QA.ProductCatalog.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using QA.Core.Models.Entities;
using System.Globalization;
using QA.Core.Models.Extensions;
using QA.Core.Cache;
using QA.ProductCatalog.ContentProviders;

namespace QA.Core.DPC.Loader.Services
{

    class ProductLocalizationService : IProductLocalizationService
    {
        private readonly ILocalizationSettingsService _settingsService;
        private readonly IContentProvider<NotificationChannel> _channelProvider;

        public ProductLocalizationService(ILocalizationSettingsService settingsService, IContentProvider<NotificationChannel> channelProvider)
        {
            _settingsService = settingsService;
            _channelProvider = channelProvider;
        }

        #region IProductLocalizationService implementation
        public Article Localize(Article product, CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            var localizationMap = _settingsService.GetSettings(product.ContentId);

            if (!localizationMap.Any())
            {
                return product;
            }

            var articleMap = CloneArticles(product);

            foreach(var item in articleMap)
            {
                var fieldGroups = GetLocalizedFields(item.Key, localizationMap);

                foreach (var fields in fieldGroups)
                {
                    LocalizedField field;
                    var currentCulture = culture;

                    do
                    {
                        field = fields.FirstOrDefault(f => f.Culture.Equals(currentCulture));

                        if (currentCulture.Equals(CultureInfo.InvariantCulture))
                        {
                            break;
                        } 
                        currentCulture = currentCulture.Parent;
                    }
                    while (field == null);


                    if (field == null)
                    {
                        throw new Exception($"Для языка {culture.DisplayName} не задано поле {fields.Key}");
                    }
                    var newField = CloneField(field.Field, field.InvariantFieldName, articleMap);
                    item.Value.Fields[newField.FieldName] = newField;
                }
            }

            return articleMap[product];
        }

        public Dictionary<CultureInfo, Article> SplitLocalizations(Article product, bool localize)
        {
            var cultures = GetCultures();
            return SplitLocalizations(product, cultures, localize);
        }

        public Dictionary<CultureInfo, Article> SplitLocalizations(Article product, CultureInfo[] cultures,
            bool localize)
        {
            return localize
                ? cultures.ToDictionary(c => c, c => Localize(product, c)) 
                : new Dictionary<CultureInfo, Article> {{ cultures[0], product }};  

        }
        public CultureInfo[] GetCultures()
        {
            return _channelProvider.GetArticles().Select(c => c.Culture).Distinct().ToArray();
        }
        #endregion

        #region Private methods
        private Dictionary<Article, Article> CloneArticles(Article article)
        {
            return article
                .GetAllArticles(true)
                .Distinct()
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
                IsReadOnly = article.IsReadOnly,
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
            var mField = field as MultiArticleField;
            var eField = field as ExtensionArticleField;
            var bField = field as BackwardArticleField;
            var vField = field as VirtualArticleField;
            var vmField = field as VirtualMultiArticleField;

            ArticleField clonnedField;

            if (pField != null)
            {
                clonnedField = new PlainArticleField
                {
                    NativeValue = pField.NativeValue,
                    Value = pField.Value,
                    PlainFieldType = pField.PlainFieldType
                };
            }
            else if (eField != null)
            {
                clonnedField = new ExtensionArticleField
                {
                    Aggregated = eField.Aggregated,
                    SubContentId = eField.SubContentId,
                    Item = eField.Item == null ? null : articleMap[eField.Item],
                    Value = eField.Value
                };
            }
            else if (sField != null)
            {
                clonnedField = new SingleArticleField
                {
                    Aggregated = sField.Aggregated,
                    SubContentId = sField.SubContentId,
                    Item = sField.Item == null ? null : articleMap[sField.Item]
                };
            }
            else if (bField != null)
            {
                clonnedField = new BackwardArticleField
                {
                    SubContentId = bField.SubContentId,
                    Items = bField.Items.ToDictionary(itm => itm.Key, itm => articleMap[itm.Value]),
                    RelationGroupName = bField.RelationGroupName
                };
            }
            else if (mField != null)
            {
                clonnedField = new MultiArticleField
                {
                    SubContentId = mField.SubContentId,
                    Items = mField.Items.ToDictionary(itm => itm.Key, itm => articleMap[itm.Value])
                };
            }
            else if (vField != null)
            {
                clonnedField = new VirtualArticleField
                {
                    Fields = vField.Fields
                };
            }
            else if (vmField != null)
            {
                clonnedField = new VirtualMultiArticleField
                {
                    VirtualArticles = vmField.VirtualArticles
                        .Select(f => CloneField(f, fieldName, articleMap) as VirtualArticleField)
                        .ToArray()
                };
            }
            else
            {
                throw new Exception($"Cant't process field {field.FieldName} for localization");
            }

            clonnedField.FieldName = fieldName;
            clonnedField.ContentId = field.ContentId;
            clonnedField.FieldId = field.FieldId;
            clonnedField.CustomProperties = field.CustomProperties;
            clonnedField.FieldDisplayName = field.FieldDisplayName;

            return clonnedField;
        }
        public IEnumerable<IGrouping<string, LocalizedField>> GetLocalizedFields(Article article, Dictionary<string, CultureInfo> localizationMap)
        {
            CultureInfo defaultCultue;

            if (!localizationMap.TryGetValue(string.Empty, out defaultCultue))
            {
                defaultCultue = CultureInfo.InvariantCulture;
            }

            var fields = from field in article
                         let suffix = localizationMap.Keys
                             .Where(l => field.FieldName.EndsWith(l))
                             .OrderByDescending(l => l.Length)
                             .FirstOrDefault()
                         let fieldName = field.FieldName
                         select new LocalizedField
                         {
                             Field = field,
                             InvariantFieldName = string.IsNullOrEmpty(suffix) ?
                                  fieldName :
                                  fieldName.Remove(fieldName.Length - suffix.Length),
                             Culture = suffix == null ?
                                  defaultCultue :
                                  localizationMap[suffix]
                         };

            var group = fields.GroupBy(f => f.InvariantFieldName);

            var invalidFields = group
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

            if (invalidFields.Any())
            {
                throw new Exception($"Поля {string.Join(", ", invalidFields)} имеют разный тип в локализованных версиях");
            }

            return group;
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
