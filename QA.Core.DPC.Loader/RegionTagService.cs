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
using QA.ProductCatalog.ContentProviders;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;
using Article = Quantumart.QP8.BLL.Article;
using QA.Core.DPC.QP.Models;

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
        private static readonly Regex XmlEntityRegex =
            new Regex(@"&lt;replacement&gt;tag=(\w+)&lt;/replacement&gt;", RegexOptions.Compiled);

        private static readonly TimeSpan CacheInterval = new TimeSpan(0, 10, 0); 
        private readonly Customer _customer;
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
            _customer = connectionProvider.GetCustomer();
            
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
        public string Replace(string text, int currentRegion, string[] exceptions = null, int depth = 0)
        {
            var tags = GetRegionTagValuesWithoutRecursion(text, new[]{ currentRegion});

            if (exceptions != null)
            {
                var ex = new HashSet<string>(exceptions);
                tags = tags.Where(t => !ex.Contains(t.Title)).ToArray();
            }

            var maxdepth = GetRecursiveDepth();
            if (depth <= maxdepth)
            {
                foreach (var tag in tags)
                {
                    foreach (var tagValue in tag.Values)
                    {
                        tagValue.Value = Replace(tagValue.Value, currentRegion, exceptions, depth + 1);
                    }
                }
            }

            var result = DefaultRegex.Replace(text, match => ReplaceMatch(tags, match));
            result = XmlEntityRegex.Replace(result, match => ReplaceMatch(tags, match));

            return result;
        }

        public string[] GetTags(string text)
        {
            var matches = new List<string>();

            var list = new List<Match>(DefaultRegex.Matches(text).Cast<Match>()
                .Concat(XmlEntityRegex.Matches(text).Cast<Match>())); 

            foreach (Match match in list)
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


        public TagWithValues[] GetRegionTagValues(string text, int[] regionIds)
        {
            return GetRegionTagValues(text, regionIds, 0).OrderBy(n => n.Title).ToArray();
        }
        
        private TagWithValues[] GetRegionTagValues(string text, int[] regionIds, int depth)
        {
            var result = new List<TagWithValues>();
            var tags = GetRegionTagValuesWithoutRecursion(text, regionIds);
            result.AddRange(tags);
            
            if (depth > GetRecursiveDepth()) return result.ToArray();

            foreach (var tagValue in tags.SelectMany(n => n.Values))
            {
                var extraTags = GetRegionTagValues(tagValue.Value, regionIds, depth + 1);
                result.AddRange(extraTags); 
            }
            
            return result.ToArray();
        }

        private TagWithValues[] GetRegionTagValuesWithoutRecursion(string text, int[] regionIds)
        {
            var textTags = GetTags(text);
            var tags = LoadTagWithValues(textTags);
            var newTags = new List<TagWithValues>();

            foreach (var tag in tags)
            {
                var regions = new HashSet<int>(regionIds);
                var regionsUsed = new HashSet<int>();
                var newTagValues = new List<TagValue>();
                
                foreach (var tagValue in tag.Values)
                {
                    // if we've found values for all regions then stop searching
                    if (regions.Count == regionsUsed.Count) break; 
                    var commonRegionIds = tagValue.RegionsId.Intersect(regions).ToArray();
                    if (commonRegionIds.Any())
                    {
                        regionsUsed.UnionWith(commonRegionIds);
                        newTagValues.Add(new TagValue()
                        {
                            RegionsId = commonRegionIds,
                            Value = tagValue.Value
                        }); 
                    }
                }

                if (regions.Count != regionsUsed.Count)
                {
                    regions.ExceptWith(regionsUsed);
                                       
                    if (regions.Any()) // if we have product regions that is not used the we'll check their parents 
                    {
                        foreach (var region in regions)
                        {
                            var parents = _regionService.GetParentsIds(region);
                            foreach (var parent in parents)
                            {
                                // if parent region has already been used for tag                                
                                if (regionsUsed.Contains(parent)) 
                                {
                                    var newTagValue = newTagValues.FirstOrDefault(n => n.RegionsId.Contains(parent));
                                    if (newTagValue == null) continue;
                                    // append region to found parent tag value
                                    newTagValue.RegionsId = newTagValue.RegionsId.Append(region).ToArray();
                                    break;

                                }
                                else
                                {
                                    // if parent has not used tag value
                                    var commonTagValue = tag.Values.FirstOrDefault(n => n.RegionsId.Contains(parent));
                                    if (commonTagValue == null) continue;
                                    // append found parent value with region itself
                                    newTagValues.Add(new TagValue()
                                    {
                                        RegionsId = new[] { region },
                                        Value = commonTagValue.Value
                                    });
                                    break;
                                }
                            }
                        }
                    }
                }

                var resultValues = ( 
                    from v in newTagValues
                    group v.RegionsId by v.Value
                    into g
                    select new TagValue() { Value = g.Key, RegionsId = g.SelectMany(n => n).ToArray() }).ToArray();

                
                newTags.Add(new TagWithValues()
                    {
                        Title = tag.Title,
                        Values = resultValues.ToArray()
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
                using (new QPConnectionScope(_customer.ConnectionString, (DatabaseType)_customer.DatabaseType))
                {
                    _articleService.LoadStructureCache();  
                    
                    var regionTagsDict = keysToLoad.Distinct().Select(n => new { CacheKey = n, Tag = n.Replace(key, "")})
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

                    return tags.Select(t => new {
                            Title = t.FieldValues.Single(n => n.Field.Name == TagTitleName).Value,
                            Values = GetTagValues(t, tagValuesRelations, tagValues),
                            Id = t.Id
                        })
                        .GroupBy(n => n.Title)
                        .Select( group => new TagWithValues
                        {
                            Title = group.Key,
                            Values = group.OrderBy(n => n.Id).First().Values
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

        private static string ReplaceMatch(TagWithValues[] tags, Match match)
        {
            if (match.Groups.Count > 1)
            {
                var group = match.Groups[1];
                if (@group.Success)
                {
                    var tag = @group.Value;


                    var replacement = tags
                        .FirstOrDefault(x => x.Title.Equals(tag, StringComparison.InvariantCulture));

                    if (replacement != null)
                    {
                        return replacement.Values.FirstOrDefault()?.Value;
                    }

                    return match.ToString(); //тег в списке исключений
                }
            }

            return string.Empty;
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
