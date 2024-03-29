﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.ImpactService
{
    public class BaseImpactCalculator
    {
        public BaseImpactCalculator(string parameterModifierName, string linkModifierName, string linkName)
        {
            ParameterModifierName = parameterModifierName;
            LinkModifierName = linkModifierName;
            LinkName = linkName;
        }

        public string ParameterModifierName { get; }

        public string LinkModifierName { get; }

        public string LinkName { get; }

        private int _maxSiblings;

        private int _maxDepth;

        public void Calculate(JObject product, JObject option)
        {
            var optionParametersRoot = option.SelectToken("Parameters");
            if (optionParametersRoot == null) return;
            var optionId = (int)option.SelectToken("Id");
            bool hasImpact = MergeLinkImpactToOption(optionParametersRoot, product, optionId);

            if (hasImpact)
            {
                var parametersRoot = product.SelectToken("Parameters");
                var optionParameters1 = optionParametersRoot.Where(n => n.SelectTokens("Modifiers.[?(@.Alias)].Alias").Select(m => m.ToString()).ToArray().Contains(ParameterModifierName)).ToArray();
                CalculateImpact(parametersRoot, optionParameters1);
            }
        }

        public void Reorder(JObject product, JObject homeRegionData)
        {
            var parametersRoot = (JArray)product.SelectToken("Parameters");

            if (parametersRoot == null) return;

            var groupOrderOverride = GetGroupOrderOverride(homeRegionData);

            SetGroupOrder(parametersRoot, groupOrderOverride);

            SetSiblingOrder(parametersRoot);

            SetNaturalOrder(parametersRoot);

            var orderedParams = parametersRoot.OrderBy(n => GetTokenOrder(n, "NaturalOrder")).ToArray();

            parametersRoot.Clear();

            foreach (var param in orderedParams)
            {
                parametersRoot.Add(param);
            }
        }

        private void SetGroupOrder(JArray parametersRoot, Dictionary<int, int> groupOrderOverride)
        {
            foreach (var p in parametersRoot)
            {
                var group = p.SelectToken("Group");
                if (group != null)
                {
                    var groupId = (int)group["Id"];
                    int order;
                    if (groupOrderOverride.TryGetValue(groupId, out order))
                    {
                        group["SortOrder"] = order;
                    }
                }
            }
        }

        public Dictionary<int, int> GetGroupOrderOverride(JObject homeRegion)
        {
            var result = new Dictionary<int, int>();
            var usagesRoot = homeRegion?.SelectToken("ParameterGroupUsages");
            if (usagesRoot != null)
            {
                foreach (var usage in usagesRoot)
                {
                    var idToken = usage.SelectToken("Group.Id");
                    var orderToken = usage.SelectToken("SortOrder");
                    if (idToken != null && orderToken != null)
                    {
                        var key = (int) idToken;
                        var value = (int) orderToken;
                        if (!result.ContainsKey(key))
                        {
                            result.Add(key, value);
                        }
                    }
                }
            }

            return result;
        }

        public static int GetLevelCapacity(int maxSiblings)
        {
            var capacity = maxSiblings / 10;
            if (maxSiblings % 10 > 0) { capacity++; }
            return capacity * 10;
        }

        private void SetNaturalOrder(JToken parametersRoot)
        {
            var capacity = GetLevelCapacity(_maxSiblings);
            for (var i = 1; i <= _maxDepth; i++)
            {
                var multiplier = GetMultiplier(_maxDepth, capacity, i);
                var levelParams = parametersRoot.SelectTokens($"[?(@.Level == {i})]").ToArray();
                foreach (var p in levelParams)
                {
                    var parentOrder = 0;

                    if (i > 1)
                    {
                        var pid = (int)p.SelectToken("Parent.Id");
                        parentOrder = (int)parametersRoot.SelectToken($"[?(@.Id == {pid})].NaturalOrder");
                    }
                    p["NaturalOrder"] = (int) p["SiblingOrder"] * multiplier + parentOrder;
                }

            }
        }

        private static int GetMultiplier(int depth, int capacity, int i)
        {
            return (depth == i) ? 1 : Enumerable.Repeat(capacity, depth - i).Aggregate((runningProduct, nextFactor) => runningProduct * nextFactor);
        }

        private static int GetTokenOrder(JToken parameter, string tokenName)
        {
            var order = parameter.SelectToken(tokenName);
            return (order == null) ? 0 : (int) order;
        }

        private void SetSiblingOrder(JToken parametersRoot, int[] ids = null, int level = 1)
        {
            var cids = new List<int>();
            if (ids == null)
            {
                var b = parametersRoot.SelectTokens("[?(@.Id)]").Where(n => n["Parent"] == null)
                    .OrderBy(n => GetTokenOrder(n, "Group.SortOrder"))
                    .ThenBy(n => GetTokenOrder(n, "Group.Id"))
                    .ThenBy(n => GetTokenOrder(n, "SortOrder"))
                    .ThenBy(n => GetTokenOrder(n, "Id"))
                    .ToArray();
                SetSiblingOrderForLevel(b, level);
                cids.AddRange(b.Select(n => (int)n["Id"]));
            }
            else
            {
                foreach (var id in ids)
                {
                    var b =
                        parametersRoot.SelectTokens("[?(@.Id)]")
                            .Where(n => n["Parent"] != null && (int) n["Parent"]["Id"] == id)
                            .OrderBy(n => GetTokenOrder(n, "SortOrder"))
                            .ThenBy(n => GetTokenOrder(n, "Id"))
                            .ToArray();
                    if (b.Any())
                    {
                        SetSiblingOrderForLevel(b, level);
                        cids.AddRange(b.Select(n => (int)n["Id"]));
                    }
                }
            }

            if (!cids.Any()) return;

            SetSiblingOrder(parametersRoot, cids.ToArray(), level + 1);
        }

        private void SetSiblingOrderForLevel(JToken[] siblings, int level)
        {
            var i = 1;

            _maxSiblings = Math.Max(_maxSiblings, siblings.Length);
            _maxDepth = Math.Max(_maxDepth, level);

            foreach (var sibling in siblings)
            {
                sibling["SiblingOrder"] = i;
                sibling["Level"] = level;
                i++;
            }
        }

        private bool MergeLinkImpactToOption(JToken optionParametersRoot, JObject product, int optionId)
        {
            var link = product.SelectTokens($"{LinkName}.[?(@.Service)]")
            .SingleOrDefault(n => (decimal)n["Service"]["Id"] == optionId);

            if (link == null)
                return false;

            var hasImpact = link.SelectTokens($"Parent.Modifiers.[?(@.Alias == '{LinkModifierName}')]").Any();

            if (hasImpact)
            {
                link["IsSelected"] = true;
                var linkParameters = link.SelectToken("Parent.Parameters");
                if (linkParameters != null)
                {
                    MergeLinkImpact((JArray)optionParametersRoot, (JArray)linkParameters);
                }

            }

            return hasImpact;
        }

        public void MergeLinkImpact(JArray optionParametersRoot, JArray linkParameters, string specialBaseParameterModifier = null)
        {
            var changedIds = new Dictionary<int, int>();

            foreach (var linkParameter in linkParameters)
            {
                // fix name mismatching in definition
                FixDefinition(linkParameter, "ParameterGroup", "Group", "ProductGroup");
                FixDefinition(linkParameter, "MatrixParent", "Parent", "ProductParent");

                var processed = false;
                if (linkParameter["BaseParameter"] != null)
                {
                    var direction = linkParameter.ExtractDirection();
                    var key = direction.GetKey();
                    var parametersToProcess = FindByKey(optionParametersRoot, key).ToArray();
                    if (!parametersToProcess.Any() && specialBaseParameterModifier != null && direction.BaseParameterModifiers.Contains(specialBaseParameterModifier))
                    {
                        key = direction.GetKey(new DirectionExclusion(new[] {specialBaseParameterModifier}));
                        parametersToProcess = FindByKey(optionParametersRoot, key).ToArray();
                    }

                    processed = parametersToProcess.Any();

                    foreach (var p in parametersToProcess)
                    {
                        var replaceKey = (int) linkParameter["Id"];
                        if (!changedIds.ContainsKey(replaceKey))
                        {
                            var toReplace = linkParameter.DeepClone();

                            if (toReplace["SortOrder"] == null && p["SortOrder"] != null)
                                toReplace["SortOrder"] = p["SortOrder"];

                            if (toReplace["Group"] == null && p["Group"] != null)
                                toReplace["Group"] = p["Group"].DeepClone();

                            if (toReplace["Parent"] == null && p["Parent"] != null)
                                toReplace["Parent"] = p["Parent"].DeepClone();

                            changedIds.Add(replaceKey, (int)p["Id"]);
                            toReplace["Id"] = p["Id"];  
                            
                            p.Replace(toReplace);
                            
                        }
                    }
                }

                if (!processed)
                {
                    ( optionParametersRoot).Add(linkParameter);
                }

                foreach (var param in optionParametersRoot)
                {
                    var mp = param.SelectToken("Parent");
                    if (mp == null) continue;

                    int changedId;
                    if (!changedIds.TryGetValue((int) mp["Id"], out changedId)) continue;

                    mp["Id"] = changedId;
                }
            }
        }

        private static void FixDefinition(JToken linkParameter, string fromName, string crossName, string toName)
        {
            var pg = linkParameter.Children<JProperty>().FirstOrDefault(n => n.Name == fromName);
            if (pg != null)
            {
                var g = linkParameter.Children<JProperty>().FirstOrDefault(n => n.Name == crossName);
                g?.Replace(new JProperty(toName, g.Value));
                pg.Replace(new JProperty(crossName, pg.Value));
            }
        }

        private void CalculateImpact(JToken parametersRoot, JToken[] optionParameters)
        {
            var parametersToAppendInsteadOfChange = new List<JToken>();

            ProcessRemove(parametersRoot, optionParameters);
            ProcessDirectParameters(parametersRoot, optionParameters);
            var changedIds = ChangeParameters(parametersRoot, optionParameters, parametersToAppendInsteadOfChange);
            ProcessPackages(parametersRoot, optionParameters);
            ProcessTarifficationSteps(parametersRoot, optionParameters);
            ProcessAppend(parametersRoot, optionParameters, parametersToAppendInsteadOfChange);
            ProcessParents(parametersRoot, changedIds);

        }

        private void ProcessParents(JToken parametersRoot, Dictionary<int, int> changedIds)
        {
            foreach (var p in parametersRoot)
            {
                var pp = p.SelectToken("Parent");
                if (pp == null) continue;

                var id = (int)pp["Id"];
                int changedId;
                var found = changedIds.TryGetValue(id, out changedId);
                if (found)
                {
                    pp["Id"] = changedId.ToString();
                    pp["Title"] = parametersRoot.SelectToken($"[?(@.Id == {changedId})].Title");
                }
                else
                {
                    if (parametersRoot.SelectToken($"[?(@.Id == {id})]") == null)
                    {
                        p.Children().OfType<JProperty>().SingleOrDefault(n => n.Name == "Parent")?.Remove();
                    }
                         
                }
            }
        }

        private void ProcessDirectParameters(JToken parametersRoot, JToken[] optionParameters)
        {
            foreach (var optionParam in optionParameters.Where(n => n.SelectToken("BaseParameter") == null))
            {
                var p  = optionParam.SelectToken("Parameter");
                if (p == null) continue;

                var modifiers = new HashSet<string>(optionParam.SelectTokens("Modifiers.[?(@.Alias)].Alias").Select(n => n.ToString()));
                var id = (int) p["Id"];
                var tariffParam = parametersRoot.SelectToken($"[?(@.Id == {id})]");

                if (modifiers.Contains("Hide"))
                {
                    tariffParam?.Remove();
                }
                else
                {
                    tariffParam["Title"] = optionParam["Title"];
                    tariffParam["Changed"] = true;
                    if (optionParam["NumValue"] != null)
                    {
                        if (tariffParam["OldNumValue"] == null && tariffParam["NumValue"] != null)
                        {
                            tariffParam["OldNumValue"] = tariffParam["NumValue"];
                        }
                        tariffParam["NumValue"] = optionParam["NumValue"];
                    }
                    if (optionParam["SortOrder"] != null)
                    {
                        tariffParam["SortOrder"] = optionParam["SortOrder"];
                    }
                }



            }
        }

        private void ProcessRemove(JToken parameters, JToken[] optionParameters)
        {
            var removeParameters =
                optionParameters.Where(
                    n => n.SelectTokens("Modifiers.[?(@.Alias)].Alias").Any(m => m.ToString() == "Remove")).ToArray();

            foreach (var param in removeParameters)
            {
                var jTokens = FindByKey(parameters, param.ExtractDirection().GetKey()).ToArray();
                foreach (var jToken in jTokens)
                {
                    jToken.Remove();
                }
            }
        }

        private void ProcessAppend(JToken parametersRoot, IEnumerable<JToken> optionParameters, List<JToken> parametersToAppendInsteadOfChange)
        {
            var parameters = (JArray) parametersRoot;
            foreach (var param in parametersToAppendInsteadOfChange)
            {
                AppendParameter(parameters, param);
            }

            var addParameters =
                optionParameters.Where(
                    n => n.SelectTokens("Modifiers.[?(@.Alias)].Alias").Any(m => m.ToString() == "Append")).ToArray();

            foreach (var param in addParameters)
            {
                AppendParameter(parameters, param);
            }
        }

        private void AppendParameter(JArray parameters, JToken param)
        {
            var key = param.ExtractDirection().GetKey();
            var clearTariffDirection = !String.IsNullOrEmpty(key) && FindByKey(parameters, key).Any();
            parameters.Add(param.PrepareForAdd(clearTariffDirection));
        }

        private Dictionary<int, int> ChangeParameters(JToken parametersRoot, IEnumerable<JToken> optionParameters, List<JToken> parametersToAdd)
        {
            var changedIds = new Dictionary<int, int>();
            foreach (var optionParam in optionParameters.Where(n => n.SelectToken("BaseParameter") != null))
            {
                var modifiers = new HashSet<string>(optionParam.SelectTokens("Modifiers.[?(@.Alias)].Alias").Select(n => n.ToString()));
                var bpModifiers = new HashSet<string>(optionParam.SelectTokens("BaseParameterModifiers.[?(@.Alias)].Alias").Select(n => n.ToString()));
                var forcedInfluence = modifiers.Contains("ForcedInfluence");
                if (modifiers.Contains("Append") || bpModifiers.Any(n => TariffDirection.SpecialModifiers.Contains(n)) && !forcedInfluence)
                    continue;

                var optionEx = GetOptionDirectionExclusion(bpModifiers);
                var key = optionParam.ExtractDirection().GetKey(optionEx);

                var tariffEx = GetTariffDirectionExclusion(bpModifiers, modifiers);
                var parametersToProcess = GetParametersToProcess(parametersRoot, key, tariffEx);
                var anyParameterProcessed = parametersToProcess.Any();

                if (anyParameterProcessed)
                {
                    changedIds.Add((int)optionParam["Id"], (int)parametersToProcess.First()["Id"]);
                }

                foreach (var param in parametersToProcess)
                {
                
                    var paramHasNumValue = param["NumValue"] != null;
                    var optionHasNumValue = optionParam["NumValue"] != null;

                    var isEqualUnits = param.SelectToken("Unit.Id")?.ToString() == optionParam.SelectToken("Unit.Id")?.ToString();
                
                    var skipProcessing = paramHasNumValue && optionHasNumValue && (decimal)param["NumValue"] <= (decimal)optionParam["NumValue"] && !forcedInfluence;
                
                    if (optionHasNumValue && !modifiers.Contains("DoNotChangeValue"))
                    {
                        if (modifiers.Contains("Add") && paramHasNumValue && isEqualUnits)
                        {
                            param["NumValue"] = (decimal)optionParam["NumValue"] + (decimal)param["NumValue"];
                            param.SetChanged();
                        }
                        else if (modifiers.Contains("Discount") && isEqualUnits)
                        {
                            if (param["OldNumValue"] == null)
                                param["OldNumValue"] = param["NumValue"];
                            param["NumValue"] = (decimal)param["NumValue"] *
                                                (1 - (decimal)optionParam["NumValue"]);
                            param.SetChanged();
                
                        }
                        else
                        {
                            if (!skipProcessing)
                            {
                                if (param["OldNumValue"] == null)
                                    param["OldNumValue"] = param["NumValue"];
                                param["NumValue"] = optionParam["NumValue"];
                                param.SetChanged();
                
                            }
                        }
                    }
                
                    if (optionParam["Value"] != null && !modifiers.Contains("DoNotChangeValue"))
                    {
                        param["Value"] = optionParam["Value"];
                        param.SetChanged();
                
                    }
                
                    if (modifiers.Contains("ChangeName"))
                    {
                        param["Title"] = optionParam["Title"];
                        param.SetChanged();
                
                    }

                    if (optionParam["Parent"] != null)
                    {
                        param["Parent"] = optionParam["Parent"];
                    }
                
                }
                
                if (!anyParameterProcessed && modifiers.Contains("AppendOrReplace"))
                        parametersToAdd.Add(optionParam);
            }

            return changedIds;


        }

        private JToken[] GetParametersToProcess(JToken parametersRoot, string key, DirectionExclusion ex)
        {
            var parametersToProcess = FindByKey(parametersRoot, key, ex).ToArray();

            if (key == TariffDirection.FeeKey)
            {
                var cityParams = FindByKey(parametersRoot, TariffDirection.CityFeeKey);
                var federalParams = FindByKey(parametersRoot, TariffDirection.FederalFeeKey);
                parametersToProcess = parametersToProcess.Union(cityParams).Union(federalParams).ToArray();
            }
            return parametersToProcess;
        }

        private static DirectionExclusion GetTariffDirectionExclusion(HashSet<string> bpModifiers, HashSet<string> modifiers)
        {
            var ex = new DirectionExclusion(new[] {"OverPackage"});

            if (bpModifiers.Contains("ZoneExpansion"))
            {
                ex.Modifiers.Add("WithinPackage");
                ex.Zone = true;
            }

            if (modifiers.Contains("ForceWithAdditionalInfluence") || modifiers.Contains("ForcedInfluence"))
            {
                ex.Modifiers.Add("WithAdditionalOption");
                ex.Modifiers.Add("WithAdditionalPackage");
            }
            return ex;
        }

        private static DirectionExclusion GetOptionDirectionExclusion(HashSet<string> bpModifiers)
        {
            var ex = new DirectionExclusion(null);

            if (bpModifiers.Contains("ZoneExpansion"))
            {
                ex.Modifiers.Add("ZoneExpansion");
                ex.Zone = true;
            }

            return ex;
        }

        private void ProcessTarifficationSteps(JToken productParametersRoot, IEnumerable<JToken> optionParameters)
        {

            var modifiers = new[] { "FirstStep", "SecondStep", "ThirdStep", "FourthStep" };
            var c = FillCollection(optionParameters, modifiers);

            ReplaceTarifficationParams(productParametersRoot, c, modifiers);
        }

        private void ReplaceTarifficationParams(JToken productParametersRoot, Dictionary<string, Dictionary<string, JToken>> c, string[] modifiers)
        {
            foreach (var key in c.Keys)
            {
                var collection = c[key];
                if (collection.Count < 2) continue;
                if (collection.All(n => n.Value["NumValue"] == null)) continue;

                var targetParams = FindByKey(productParametersRoot, key).ToArray();
                if (!targetParams.Any()) continue;

                var targetParam = targetParams[0];
                var numValue = targetParam["NumValue"];
                var order = targetParam["Order"];
                var parent = targetParam["Parent"]?.DeepClone();
                var group = targetParam["Group"]?.DeepClone();
                var unit = targetParam["Unit"]?.DeepClone();
                targetParam.Remove();

                var i = 0;
                foreach (var modifier in modifiers)
                {
                    if (!collection.ContainsKey(modifier)) continue;

                    var optionParam = collection[modifier];

                    if (optionParam["NumValue"] == null)
                    {
                        optionParam["NumValue"] = numValue;
                    }
                    if (order != null)
                    {
                        optionParam["Order"] = (decimal)order + i;
                    }
                    if (parent != null && optionParam["Parent"] == null)
                    {
                        optionParam["Parent"] = parent;
                    }
                    if (group != null)
                    {
                        optionParam["Group"] = group;
                    }
                    if (unit != null)
                    {
                        optionParam["Unit"] = unit;
                    }

                    i++;
                    optionParam.SetChanged();

                    ((JArray)productParametersRoot).Add(optionParam);
                }
            }
        }

        private void ProcessPackages(JToken productParametersRoot, IEnumerable<JToken> optionParameters)
        {
            var modifiers = new[] {"WithinPackage", "OverPackage"};
            var c = FillCollection(optionParameters, modifiers);
            ReplaceTarifficationParams(productParametersRoot, c, modifiers);
        }

        private static Dictionary<string, Dictionary<string, JToken>> FillCollection(IEnumerable<JToken> optionParameters, string[] modifiers)
        {
            var c = new Dictionary<string, Dictionary<string, JToken>>();
            var a = optionParameters.ToArray();
            foreach (var modifier in modifiers)
            {
                var parameters =
                    a.Where(
                        n => n.SelectTokens("BaseParameterModifiers.[?(@.Alias)].Alias").Any(m => m.ToString() == modifier)).ToArray();
                AddParametersToCollection(parameters, c, modifier);
            }
            return c;
        }

        private static void AddParametersToCollection(JToken[] withinPackages, Dictionary<string, Dictionary<string, JToken>> c, string modifier)
        {
            foreach (var wp in withinPackages)
            {
                var key = wp.ExtractDirection().GetKey(true);

                if (string.IsNullOrEmpty(key)) continue;

                if (!c.ContainsKey(key))
                {
                    c.Add(key, new Dictionary<string, JToken>());
                }

                if (!c[key].ContainsKey(modifier))
                {
                    c[key].Add(modifier, wp);
                }
            }
        }

        public IEnumerable<JToken> FindByKey(JToken parametersRoot, string key, bool excludeSpecial = false,
            bool excludeZone = false)
        {
            return FindByKey(parametersRoot, key,
                new DirectionExclusion((excludeSpecial) ? TariffDirection.SpecialModifiers : null)
                {
                    Zone = excludeZone
                });
        }

        public IEnumerable<JToken> FindByKey(JToken parametersRoot, string key, DirectionExclusion exclusion)
        {
            var defaultResult = Enumerable.Empty<JToken>();
            if (!string.IsNullOrEmpty(key) && parametersRoot != null)
            {
                var abc =
                    parametersRoot.SelectTokens("[?(@.BaseParameter)]").Where(n => n.ExtractDirection().GetKey(exclusion) == key);



                return abc;
            }
            return defaultResult;
        }

        public virtual JObject Calculate(JObject tariff, JObject[] options, JObject homeRegionData)
        {
            foreach (var option in options)
            {
                Calculate(tariff, option);
            }
            Reorder(tariff, homeRegionData);
            return tariff;
        }

        public virtual IEnumerable<JToken> FilterServicesOnProduct(JObject product, IEnumerable<int> excludeIds = null, JObject homeRegionData = null)
        {
            var root = product.SelectToken(LinkName);
            if (root == null) yield break;
            var homeRegion = homeRegionData?.SelectToken("Alias")?.ToString();
            foreach (var elem in (JArray)root)
            {
                var hasModifier =
                    elem.SelectTokens("Parent.Modifiers.[?(@.Alias)].Alias")
                        .Select(m => m.ToString())
                        .Contains(LinkModifierName);
                var isArchive = elem.SelectTokens("Service.Modifiers.[?(@.Alias)].Alias")
                    .Select(m => m.ToString())
                    .Contains("Archive");
                
                var serviceId = (int) elem.SelectToken("Service.Id");
                var toExclude = excludeIds?.Contains(serviceId) ?? false;

                if (hasModifier && !isArchive && !toExclude)
                {
                    var regionAliases = new HashSet<string>(elem.SelectTokens("Service.Regions.[?(@.Alias)].Alias").Select(m => m.ToString()));
                    var satisfyRegion = string.IsNullOrEmpty(homeRegion) 
                                        || !regionAliases.Any() 
                                        || regionAliases.Contains(homeRegion);
                        
                     if (satisfyRegion) yield return elem;
                }

            }
        }

        protected IEnumerable<JToken> AppendParents(IEnumerable<JToken> root, IEnumerable<JToken> inParams)
        {
            var countryParamIds = new HashSet<int>(inParams.Select(n => (int)n["Id"]));
            var parentParamIds = new HashSet<int>(inParams
                .Select(n => n.SelectToken("Parent.Id"))
                .Where(n => n != null)
                .Select(n => (int)n)
            );

            var parentParams = root.Where(n => parentParamIds.Contains((int)n["Id"]) && !countryParamIds.Contains((int)n["Id"])).ToArray();
            return inParams.Union(parentParams).ToArray();
        }

        private bool IsProductForCorpSite(JObject product)
        {
            return product.SelectTokens("Modifiers.[?(@.Alias)].Alias").Select(m => m.ToString()).Contains("IsForCorpSite")
                            || product.SelectTokens("MarketingProduct.Modifiers.[?(@.Alias)].Alias").Select(m => m.ToString()).Contains("IsForCorpSite");
        }

        public void SaveServicesOnProduct(JObject product, IEnumerable<JToken> servicesOnProduct)
        {
            if (product == null) return;
            var root = product.SelectToken(LinkName);
            if (root == null)
            {
                product.Add(new JProperty(LinkName, servicesOnProduct));
            }
            else
            {
                var filtered = servicesOnProduct.ToDictionary(n => (int)n["Id"], m => m);
                if (IsProductForCorpSite(product))
                {
                    foreach (var srv in filtered.Values.Select(n => n.SelectToken("Service")).Where(n => n != null))
                    {
                        if (srv["CorpLink"] != null)
                        {
                            srv["Link"] = srv["CorpLink"];
                        }
                    }
                }

                var toDelete = ((JArray) root).Where(n => !filtered.ContainsKey((int) n["Id"])).ToArray();

                foreach (var item in toDelete)
                {
                    item.Remove();
                }
            }
        }
        
        public int[] FindServicesOnProduct(JObject product, string link, string modifier)
        {
            return product?.SelectTokens($"{link}.[?(@.Service)]")
                       .Where(n => n.SelectTokens($"Parent.Modifiers.[?(@.Alias == '{modifier}')]").Any())
                       .Select(n => (int) n.SelectToken("Service.Id"))
                       .ToArray() ?? new int[] { };
        }

        protected static void ProcessRemoveModifier(JArray scaleParameters)
        {
            var toRemove =
                scaleParameters.Where(
                        n => n.SelectTokens("Modifiers.[?(@.Alias)].Alias").Select(m => m.ToString()).Contains("Remove"))
                    .ToArray();

            foreach (var t in toRemove)
            {
                t.Remove();
            }
        }
    }
}