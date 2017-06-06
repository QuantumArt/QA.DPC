﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using QA.Core.Cache;
using QA.ProductCatalog.Infrastructure;
using QA.Core.Models.Entities;
using System.Configuration;
using QA.Core.DPC.Loader.Resources;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.QP.Services;
using Quantumart.QP8.BLL;

namespace QA.Core.DPC.Loader
{
    public class RegionTagService : IRegionTagReplaceService
    {
        #region Константы
        private const string FIELD_TITLE = "Title";
        private const string FIELD_REGION_TAG = "RegionTag";
        private const string FIELD_VALUE = "Value";
        private const string FIELD_REGIONS = "Regions";
        #endregion

        #region Глобальные переменные
        private readonly IVersionedCacheProvider _cacheProvider;
        private readonly ISettingsService _settingsService;
        private readonly IRegionService _regionService;
        private static readonly Regex DefaultRegex = new Regex(@"[<\[]replacement[>\]]tag=(\w+)[<\[]/replacement[>\]]", RegexOptions.Compiled);
        private static readonly TimeSpan _cacheInterval = new TimeSpan(0, 10, 0); 
        private readonly string _connectionString;
        private readonly ICacheItemWatcher _cacheItemWatcher;
        private readonly IArticleService _articleService;

        #endregion

        #region Конструкторы
        public RegionTagService(IVersionedCacheProvider cacheProvider, ISettingsService settingsService, IRegionService regionService, ICacheItemWatcher cacheItemWatcher,
            IArticleService articleService, IConnectionProvider connectionProvider)
        {
            _cacheProvider = cacheProvider;
            _settingsService = settingsService;
            _regionService = regionService;
            _cacheItemWatcher = cacheItemWatcher;
            _articleService = articleService;

            _connectionString = connectionProvider.GetConnection();;
        }
        #endregion

        #region IRegionTagReplaceService
        public string Replace(string text, int currenrRegion, string[] exceptions = null)
        {
            // Сделал замену универсальной, на основании групп регулярного выражения
            List<RegionTag> tags;
            using (new QPConnectionScope(_connectionString))
            {
                tags = GetRegionTags(currenrRegion);
            }

            if (exceptions != null)
            {
                tags = tags.Where(t => !exceptions.Contains(t.Tag)).ToList();
            }

            int depth = GetRecursiveDepth();

            for (int i = 0; i < depth; i++)
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
            List<string> matches = new List<string>();

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
            var ragTagsContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.REGIONAL_TAGS_CONTENT_ID));
            var ragTagsValuesContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.REGIONAL_TAGS_VALUES_CONTENT_ID));
            var key = "RegionTagService.GetRegionTags_currentRegion: " + currentRegion;
            var tags = new string[] { ragTagsContentId.ToString(), ragTagsValuesContentId.ToString() };

            // выбираем сущности только для данного региона.
            // меньше элементов = быстрее получаем из кеша
            return (List<RegionTag>)_cacheProvider.GetOrAdd(key, tags, _cacheInterval, () =>
            {
				List<RegionTag> result = new List<RegionTag>();
                _articleService.LoadStructureCache();
                List<RegionTag> rtags = GetAllRegionTags(_articleService);
                List<RegionTagValue> tagvalues = GetAllRegionTagsValues(_articleService);

                List<int> ids = new List<int>();
                ids.Add(currentRegion);
                ids.AddRange(_regionService.GetParentsIds(currentRegion));

                foreach (RegionTagValue v in tagvalues)
                {
                    RegionTag t = rtags.Where(x => x.Id == v.RegionTagId).FirstOrDefault();
                    if (t == null)
                        continue;

                    if (!v.RegionsId.Any(z => ids.Contains(z)))
                        continue;

                    //Исключение дублирования тегов: если, например, задан тег для России и Москвы, приоритет должен быть отдан тегу Мосвы и он должен быть один в результате
                    //RegionId при этом устанавливается в фактический регион значения тега (т.е. Москва)
                    var existingTagRegion = result.Where(x => x.Id == v.RegionTagId).FirstOrDefault(); //Регион уже добавленный в рузультат. Если он есть, будем сравнивать текущий на приоритетность
                    int index = 0;
                    if (existingTagRegion != null)   //Ранее было найдено значение тега для данного региона или его родителя
                    {
                        bool hasMorePriority = false; //ранее было найдено значение тега для региона этого же уровня (или ниже), т.е. с таким или более высоким приоритетом
                        while (ids.Count != index)
                        {
                            if (ids[index] == existingTagRegion.RegionId) //уже найден для данного региона
                            {
                                hasMorePriority = true;
                                break;
                            }
                            if (v.RegionsId.Any(x => x == ids[index])) break;
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
                            if (v.RegionsId.Any(x => x == ids[index])) break;
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
                return result;
            });

        }

        public TagWithValues[] GetRegionTagValues(string text, IEnumerable<int> regionIds)
        {
            string[] textTags = GetTags(text);

            return regionIds
                .SelectMany(GetRegionTags)
                .Where(x => textTags.Contains(x.Tag))
                .GroupBy(x => x.Tag)
                .Select(x => new TagWithValues
                {
                    Title = x.Key,
                    Values = x.GroupBy(y => y.Value).Select(y => new TagValue {Value = y.Key, RegionsId = y.Select(z => z.RegionId).ToArray()}).ToArray()
                })
                .ToArray();
        }

        #endregion

        #region Закрытые методы
        private string Replace(string text, List<RegionTag> tags)
        {         
            var result = DefaultRegex.Replace(text, (MatchEvaluator)(match =>
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
            }));

            return result;
        }

        /// <summary>
        /// Получение всех региональных тегов (контент Региональные теги). Без значений.
        /// </summary>
        /// <param name="articleService"></param>
        /// <returns></returns>
        private List<RegionTag> GetAllRegionTags(IArticleService articleService)
        {
            var regTagsContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.REGIONAL_TAGS_CONTENT_ID));
            var key = "RegionTagService.GetAllRegionTags";
            var tags = new string[] { regTagsContentId.ToString() };

            return (List<RegionTag>)_cacheProvider.GetOrAdd(key, tags, _cacheInterval, () =>
                {
                    List<RegionTag> res = new List<RegionTag>();
                    var list = articleService.List(regTagsContentId, null).Where(x => !x.Archived && x.Visible);
                    if (list != null)
                    {
                        res.AddRange(list.Select(x => new RegionTag() { Id = x.Id, Tag = x.FieldValues.Where(a => a.Field.Name == FIELD_TITLE).Select(a => a.Value).FirstOrDefault() }));
                    }

                    return res;
                });
        }

        /// <summary>
        /// Получение всех региональных тегов (контент Региональные теги). Без названий тегов.
        /// </summary>
        /// <param name="articleService"></param>
        /// <returns></returns>
        private List<RegionTagValue> GetAllRegionTagsValues(IArticleService articleService)
        {
            var ragTagsValuesContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.REGIONAL_TAGS_VALUES_CONTENT_ID));
            var key = "RegionTagService.GetAllRegionTagsValues";
            var tags = new string[] { ragTagsValuesContentId.ToString() };

            return (List<RegionTagValue>)_cacheProvider.GetOrAdd(key, tags, _cacheInterval, () =>
            {
                List<RegionTagValue> res = new List<RegionTagValue>();
                var list = articleService.List(ragTagsValuesContentId, null).Where(x => !x.Archived && x.Visible);
                if (list != null)
                {
                    foreach (var item in list)
                    {
                        var regionTagId = item.FieldValues.Where(a => a.Field.Name == FIELD_REGION_TAG).Select(a => a.Value).FirstOrDefault();
                        res.Add(new RegionTagValue()
                        {
                            Id = item.Id,
                            Value = item.FieldValues.Where(a => a.Field.Name == FIELD_VALUE).Select(a => a.Value).FirstOrDefault(),
                            RegionsId = item.FieldValues.Where(a => a.Field.Name == FIELD_REGIONS).SelectMany(a => a.RelatedItems).ToArray(),
                            RegionTagId = string.IsNullOrEmpty(regionTagId) ? 0 : int.Parse(regionTagId)
                        }); ;
                    }
                }

                return res;
            });
        }

        private int GetRecursiveDepth()
        {
            int depth = 0;
            string value = _settingsService.GetSetting(SettingsTitles.REGIONAL_TAGS_RECURSIVE_DEPTH);

            if (value != null && int.TryParse(value, out depth))
            {
                return depth;
            }

            return 0;
        }
        #endregion
    }
}
