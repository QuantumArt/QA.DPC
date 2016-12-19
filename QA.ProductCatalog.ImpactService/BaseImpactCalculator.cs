using System;
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
            var optionId = (int)option.SelectToken("Id");
            bool hasImpact = MergeLinkImpactToOption(optionParametersRoot, product, optionId);

            if (hasImpact)
            {
                var parametersRoot = product.SelectToken("Parameters");
                var optionParameters1 = optionParametersRoot.Where(n => n.SelectTokens("Modifiers.[?(@.Alias)].Alias").Select(m => m.ToString()).ToArray().Contains(ParameterModifierName)).ToArray();
                CalculateImpact(parametersRoot, optionParameters1);
            }
        }

        public void Reorder(JObject product)
        {
            var parametersRoot = (JArray)product.SelectToken("Parameters");

            SetSiblingOrder(parametersRoot);

            SetNaturalOrder(parametersRoot);

            var orderedParams = parametersRoot.OrderBy(n => GetTokenOrder(n, "NaturalOrder")).ToArray();

            parametersRoot.Clear();

            foreach (var param in orderedParams)
            {
                parametersRoot.Add(param);
            }

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
                var linkParameters = link.SelectTokens("Parent.Parameters[?(@.Id)]");

                foreach (var linkParameter in linkParameters)
                {
                    bool processed = false;
                    if (linkParameter["BaseParameter"] != null)
                    {
                        var key = linkParameter.ExtractDirection().GetKey(false);
                        var parametersToProcess = FindByKey(optionParametersRoot, key, false).ToArray();
                        processed = parametersToProcess.Any();
                        foreach (var p in parametersToProcess)
                        {
                            p.Replace(linkParameter.DeepClone());
                        }
                    }

                    if (!processed)
                    {
                        ((JArray) optionParametersRoot).Add(linkParameter);
                    }

                }
            }

            return hasImpact;
        }

        private void CalculateImpact(JToken parametersRoot, JToken[] optionParameters1)
        {
            var parametersToAppendInsteadOfChange = new List<JToken>();
            ProcessRemove(parametersRoot, optionParameters1);
            ChangeParameters(parametersRoot, optionParameters1, parametersToAppendInsteadOfChange);
            ProcessPackages(parametersRoot, optionParameters1);
            ProcessTarifficationSteps(parametersRoot, optionParameters1);
            ProcessAppend((JArray) parametersRoot, optionParameters1, parametersToAppendInsteadOfChange);
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

        private void ProcessAppend(JArray parameters, IEnumerable<JToken> optionParameters, List<JToken> parametersToAppendInsteadOfChange)
        {
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

        private void ChangeParameters(JToken parametersRoot, IEnumerable<JToken> optionParameters, List<JToken> parametersToAdd)
        {
            foreach (var optionParam in optionParameters.Where(n => !n.SelectTokens("BaseParameterModifiers.[?(@.Alias)].Alias").Any(m => TariffDirection.SpecialModifiers.Contains(m.ToString()))))
            {
                var modifiers = new HashSet<string>(optionParam.SelectTokens("Modifiers.[?(@.Alias)].Alias").Select(n => n.ToString()));
                if (!modifiers.Contains("Append"))
                {
                    var key = optionParam.ExtractDirection().GetKey();
                    var parametersToProcess = FindByKey(parametersRoot, key).ToArray();
                    var appendOrReplace = modifiers.Contains("AppendOrReplace");
                    var optionHasNumValue = optionParam["NumValue"] != null;
                    var optionHasValue = optionParam["Value"] != null;
                    foreach (var param in parametersToProcess)
                    {
                        var productHasNumValue = param["NumValue"] != null;
                        var skipProcessing = productHasNumValue && optionHasNumValue && (decimal)param["NumValue"] < (decimal)optionParam["NumValue"] && !modifiers.Contains("ForcedInfluence");

                        if (optionHasNumValue && !modifiers.Contains("DoNotChangeValue"))
                        {
                            if (modifiers.Contains("Add") && productHasNumValue)
                            {
                                param["NumValue"] = (decimal)optionParam["NumValue"] + (decimal)param["NumValue"];
                                param.SetChanged();
                            }
                            else if (modifiers.Contains("Discount"))
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

                        if (optionHasValue && !modifiers.Contains("DoNotChangeValue"))
                        {
                            param["Value"] = optionParam["Value"];
                            param.SetChanged();

                        }

                        if (modifiers.Contains("ChangeName"))
                        {
                            param["Title"] = optionParam["Title"];
                            param.SetChanged();

                        }



                    }

                    if (!parametersToProcess.Any() && appendOrReplace)
                        parametersToAdd.Add(optionParam);


                }
            }


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
                if (collection.Count >= 2)
                {
                    var targetParams = FindByKey(productParametersRoot, key).ToArray();
                    if (targetParams.Any())
                    {
                        var targetParam = targetParams[0];
                        var numValue = targetParam["NumValue"];
                        var order = targetParam["Order"];
                        var parent = targetParam["Parent"]?.DeepClone();
                        var group = targetParam["Group"]?.DeepClone();
                        var unit = targetParam["Unit"]?.DeepClone();
                        targetParam.Remove();

                        int i = 0;
                        foreach (var modifier in modifiers)
                        {
                            if (collection.ContainsKey(modifier))
                            {
                                var param = collection[modifier];
                                if (param["NumValue"] == null)
                                    param["NumValue"] = numValue;
                                if (order != null)
                                    param["Order"] = (decimal) order + i;
                                if (parent != null)
                                    param["Parent"] = parent;
                                if (group != null)
                                    param["Group"] = group;
                                if (unit != null)
                                    param["Unit"] = unit;
                                i++;
                                param.SetChanged();

                                ((JArray)productParametersRoot).Add(param);
                            }
                        }
                    }
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
                var key = wp.ExtractDirection().GetKey();
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

        public IEnumerable<JToken> FindByKey(JToken parametersRoot, string key, bool excludeSpecial = true, bool excludeZone = false)
        {
            var defaultResult = Enumerable.Empty<JToken>();
            if (!string.IsNullOrEmpty(key) && parametersRoot != null)
            {
                return
                    parametersRoot.SelectTokens("[?(@.BaseParameter)]").Where(n => n.ExtractDirection().GetKey(excludeSpecial, excludeZone) == key);
            }
            return defaultResult;
        }
    }
}