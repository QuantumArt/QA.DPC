using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using QA.Core.Cache;
using QA.ProductCatalog.Infrastructure;
using QA.Core.Models.Entities;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.QP.Services;
using QA.Core.ProductCatalog.Actions.Services;
using Quantumart.QP8.BLL;
using Article = Quantumart.QP8.BLL.Article;

namespace QA.Core.DPC.Loader
{
    public class RegionTagService : IRegionTagReplaceService
    {

        #region Глобальные переменные
        private readonly IVersionedCacheProvider _cacheProvider;
        private readonly ISettingsService _settingsService;
        private readonly IRegionService _regionService;
        private static readonly Regex DefaultRegex =
            new Regex(@"[<\[]replacement[>\]]tag=(\w+)[<\[]/replacement[>\]]", RegexOptions.Compiled);
        private static readonly TimeSpan CacheInterval = new TimeSpan(0, 10, 0); 
        private readonly string _connectionString;
        private readonly IArticleService _articleService;
        private readonly IFieldService _fieldService;

        #endregion

        #region Конструкторы
        public RegionTagService(IVersionedCacheProvider cacheProvider, ISettingsService settingsService,
            IRegionService regionService, IArticleService articleService, IFieldService fieldService,
            IConnectionProvider connectionProvider)
        {
            _cacheProvider = cacheProvider;
            _settingsService = settingsService;
            _regionService = regionService;
            _articleService = articleService;
            _fieldService = fieldService;
            _connectionString = connectionProvider.GetConnection();
            
            TagsContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.REGIONAL_TAGS_CONTENT_ID));
            TagValuesContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.REGIONAL_TAGS_VALUES_CONTENT_ID));
            RegionsContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.REGIONS_CONTENT_ID));
            TagTitleName = _settingsService.GetSetting(SettingsTitles.REGIONAL_TAGS_KEY_FIELD_NAME) ?? "Title";
            TagValueName = _settingsService.GetSetting(SettingsTitles.REGIONAL_TAGS_VALUES_VALUE_FIELD_NAME) ?? "Value";
            
        }

        private int RegionsContentId { get; }

        private int TagsContentId { get; }

        private int TagValuesContentId { get; }
        
        private string TagTitleName { get; }
        
        private string TagValueName { get; }

        #endregion

        #region IRegionTagReplaceService
        public string Replace(string text, int currentRegion, string[] exceptions = null)
        {
            // Сделал замену универсальной, на основании групп регулярного выражения
            List<RegionTag> tags;

            tags = GetRegionTags(currentRegion);

            if (exceptions != null)
            {
                tags = tags.Where(t => !exceptions.Contains(t.Tag)).ToList();
            }

            var depth = GetRecursiveDepth();

            for (var i = 0; i < depth; i++)
            {
                foreach (var tag in tags)
                {
                    tag.Value = Replace(tag.Value, tags);
                }
            }

            return Replace(text, tags);
        }

        public string[] GetTags(string text)
        {
            var matches = new List<string>();

            foreach (Match match in DefaultRegex.Matches(text))
            {
                if (match.Groups.Count > 1)
                {
                    var group = match.Groups[1];
                    if (group.Success)
                    {
                        matches.Add(group.Value);
                    }
                }
            }
            return matches.Distinct().ToArray();
        }

        public List<RegionTag> GetRegionTags(int currentRegion)
        {

            var key = "RegionTagService.GetRegionTags_currentRegion: " + currentRegion;
            var tags = new[] { TagsContentId.ToString(), TagValuesContentId.ToString() };

            // выбираем сущности только для данного региона.
            // меньше элементов = быстрее получаем из кеша
            return _cacheProvider.GetOrAdd(key, tags, CacheInterval, () =>
            {
                var result = new List<RegionTag>();
                using (new QPConnectionScope(_connectionString))
                {
                    _articleService.LoadStructureCache();
                    var rtags = GetAllRegionTags(_articleService);
                    var tagvalues = GetAllRegionTagsValues(_articleService);
                    var ids = new List<int> {currentRegion};
                    ids.AddRange(_regionService.GetParentsIds(currentRegion));

                    foreach (var v in tagvalues)
                    {
                        var t = rtags.FirstOrDefault(x => x.Id == v.RegionTagId);
                        if (t == null)
                            continue;

                        if (!v.RegionsId.Any(z => ids.Contains(z)))
                            continue;

                        //Исключение дублирования тегов: если, например, задан тег для России и Москвы, приоритет должен быть отдан тегу Мосвы и он должен быть один в результате
                        //RegionId при этом устанавливается в фактический регион значения тега (т.е. Москва)
                        var existingTagRegion =
                            result.FirstOrDefault(x =>
                                x.Id == v
                                    .RegionTagId); //Регион уже добавленный в рузультат. Если он есть, будем сравнивать текущий на приоритетность
                        var index = 0;
                        if (existingTagRegion != null
                        ) //Ранее было найдено значение тега для данного региона или его родителя
                        {
                            var hasMorePriority =
                                false; //ранее было найдено значение тега для региона этого же уровня (или ниже), т.е. с таким или более высоким приоритетом
                            while (ids.Count != index)
                            {
                                var id = ids[index];
                                if (id == existingTagRegion.RegionId) //уже найден для данного региона
                                {
                                    hasMorePriority = true;
                                    break;
                                }

                                if (v.RegionsId.Any(x => x == id))
                                {
                                    break;
                                }

                                index++;
                            }

                            if (!hasMorePriority)
                            {
                                existingTagRegion.Value = v.Value;
                                existingTagRegion.RegionId = ids[index];
                            }
                        }
                        else
                        {
                            while (ids.Count != index)
                            {
                                var id = ids[index];
                                if (v.RegionsId.Any(x => x == id))
                                {
                                    break;
                                }

                                index++;
                            }

                            result.Add(new RegionTag
                            {
                                Id = t.Id,
                                Tag = t.Tag,
                                Value = v.Value,
                                RegionId = ids[index]
                            });
                        }

                    }
                }
                return result;
            });
        }

        public TagWithValues[] GetRegionTagValues(string text, int[] regionIds)
        {
            var textTags = GetTags(text);
            var newTags = new List<TagWithValues>();

            var tags = LoadTagWithValues(textTags);
            foreach (var tag in tags)
            {
                var newTagValues = new List<TagValue>();
                foreach (var tagValue in tag.Values)
                {
                    var commonRegionIds = tagValue.RegionsId.Intersect(regionIds).ToArray();
                    if (commonRegionIds.Any())
                    {
                        newTagValues.Add(new TagValue()
                        {
                            RegionsId = commonRegionIds,
                            Value = tagValue.Value
                        }); 
                    }
                }

                newTags.Add(new TagWithValues()
                    {
                        Title = tag.Title,
                        Values = newTagValues.ToArray()
                    }
                );
            }

            return newTags.ToArray();
        }

        private IEnumerable<TagWithValues> LoadTagWithValues(IEnumerable<string> regionTags)
        {
            const string key = "RegionTagService.GetRegionTag_";
            var cacheTags = new[] { TagsContentId.ToString(), TagValuesContentId.ToString() };

            var keys = regionTags.Select(n => key + n).ToArray();
            return _cacheProvider.GetOrAddValues(keys, string.Empty, cacheTags, CacheInterval, (keysToLoad) =>
            {
                using (new QPConnectionScope(_connectionString))
                {
                    _articleService.LoadStructureCache();  
                    
                    var regionTagsDict = keysToLoad.Select(n => new { CacheKey = n, Tag = n.Replace(key, "")})
                        .ToDictionary(n => n.Tag, m => m.CacheKey);

                    var regionTagsStr = string.Join(", ", regionTagsDict.Keys.Select(n => $"'{n.Replace("'", "''")}'"));
                    var condition = $"c.{TagTitleName} in ({regionTagsStr})";
                    var tags = _articleService.List(TagsContentId, null, true, condition).ToArray();
                    var tagIds = tags.Select(n => n.Id).ToArray();

                    var tagValuesFields = _fieldService.List(TagValuesContentId).ToArray();
                    var tagRelationFieldId = tagValuesFields.Single(n => n.RelateToContentId == TagsContentId).Id;
                    var tagValuesRelations =
                        _articleService.GetRelatedItems(new[] {tagRelationFieldId}, tagIds)[tagRelationFieldId];

                    var tagValueIds = tagValuesRelations.Values.SelectMany(n => n).Distinct().ToArray();
                    var tagValues = _articleService.List(TagValuesContentId, tagValueIds).ToDictionary(n => n.Id, m => m);

                    return tags.Select(t => new TagWithValues() {
                            Title = t.FieldValues.Single(n => n.Field.Name == TagTitleName).Value,
                            Values = GetTagValues(t, tagValuesRelations, tagValues)
                        })
                        .ToDictionary(n => regionTagsDict[n.Title], m => m);
                }
            });
        }

        private TagValue[] GetTagValues(Article t, Dictionary<int, List<int>> tagValuesRelations,  Dictionary<int, Article> tagValues)
        {
            return tagValuesRelations.TryGetValue(t.Id, out var tagValueRelation)
                ? tagValueRelation.Select(n => tagValues[n]).Select(GetTagValue).OrderBy(n => n.RegionsId.FirstOrDefault()).ToArray()
                : new TagValue[] { };
        }

        private TagValue GetTagValue(Article input)
        {
            return new TagValue()
            {
                Value = input.FieldValues.Single(m => m.Field.Name == TagValueName).Value,
                RegionsId = input.FieldValues.Single(m => m.Field.RelateToContentId == RegionsContentId)
                    .RelatedItems

            };
        }

        #endregion

        #region Закрытые методы
        private string Replace(string text, List<RegionTag> tags)
        {         
            var result = DefaultRegex.Replace(text, match =>
            {
                if (match.Groups.Count > 1)
                {
                    var group = match.Groups[1];
                    if (group.Success)
                    {
                        var tag = group.Value;


                        var replacement = tags
                            .FirstOrDefault(x => x.Tag.Equals(tag, StringComparison.InvariantCulture));

                        if (replacement != null)
                        {
                            return replacement.Value;
                        }
                        else
                        {
                            return match.ToString(); //тег в списке исключений
                        }
                    }
                }
                return string.Empty;
            });

            return result;
        }

        /// <summary>
        /// Получение всех региональных тегов (контент Региональные теги). Без значений.
        /// </summary>
        /// <param name="articleService"></param>
        /// <returns></returns>
        private List<RegionTag> GetAllRegionTags(IReadOnlyArticleService articleService)
        {
            var regTagsContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.REGIONAL_TAGS_CONTENT_ID));
            const string key = "RegionTagService.GetAllRegionTags";
            var tags = new[] { regTagsContentId.ToString() };

            return _cacheProvider.GetOrAdd(key, tags, CacheInterval, () =>
            {
                var res = new List<RegionTag>();
                var list = articleService.List(regTagsContentId, null).Where(x => !x.Archived && x.Visible);
                res.AddRange(list.Select(x => new RegionTag()
                {
                    Id = x.Id, 
                    Tag = x.FieldValues.Where(a => a.Field.Name == TagTitleName).Select(a => a.Value).FirstOrDefault()
                }));

                return res;
            });
        }

        /// <summary>
        /// Получение всех региональных тегов (контент Региональные теги). Без названий тегов.
        /// </summary>
        /// <param name="articleService"></param>
        /// <returns></returns>
        private IEnumerable<RegionTagValue> GetAllRegionTagsValues(IReadOnlyArticleService articleService)
        {
            var ragTagsValuesContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.REGIONAL_TAGS_VALUES_CONTENT_ID));
            const string key = "RegionTagService.GetAllRegionTagsValues";
            var tags = new[] { ragTagsValuesContentId.ToString() };

            return _cacheProvider.GetOrAdd(key, tags, CacheInterval, () =>
            {
                var res = new List<RegionTagValue>();
                var list = articleService.List(ragTagsValuesContentId, null).Where(x => !x.Archived && x.Visible);
                foreach (var item in list)
                {
                    var regionTagId = item.FieldValues.Where(a => a.Field.RelateToContentId == TagsContentId).Select(a => a.Value).FirstOrDefault();
                    res.Add(new RegionTagValue()
                    {
                        Id = item.Id,
                        Value = item.FieldValues.Where(a => a.Field.Name == TagValueName).Select(a => a.Value).FirstOrDefault(),
                        RegionsId = item.FieldValues.Where(a => a.Field.RelateToContentId == RegionsContentId).SelectMany(a => a.RelatedItems).ToArray(),
                        RegionTagId = string.IsNullOrEmpty(regionTagId) ? 0 : int.Parse(regionTagId)
                    }); 
                }

                return res;
            });
        }

        private int GetRecursiveDepth()
        {
            var value = _settingsService.GetSetting(SettingsTitles.REGIONAL_TAGS_RECURSIVE_DEPTH);

            if (value != null && int.TryParse(value, out var depth))
            {
                return depth;
            }

            return 0;
        }
        #endregion
    }
}
