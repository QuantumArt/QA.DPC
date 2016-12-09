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

        public void Calculate(JObject product, JObject option)
        {
            var optionParameters = (JArray)option.SelectToken("product.Parameters");
            var optionId = (int)option.SelectToken("product.Id");
            var linkParameters = product.SelectTokens($"product.{LinkName}.[?(@.Service)]")
                .Where(n => (int)n["Service"]["Id"] == optionId)
                .Where(n => n.SelectTokens("Parent.Modifiers.[?(@.Alias)].Alias").Select(m => m.ToString()).ToArray().Contains(LinkModifierName))
                .Select(n => n["Parent"]["Parameters"]);
            MergeLinkWithOption(optionParameters, linkParameters);
            ProcessBasicImpact(product, option);
        }

        private void ProcessBasicImpact(JObject product, JObject option)
        {
            var parametersRoot = product.SelectToken("product.Parameters");
            var optionParameters = option.SelectToken("product.Parameters").Where(n => n.SelectTokens("Modifiers.[?(@.Alias)].Alias").Select(m => m.ToString()).ToArray().Contains(ParameterModifierName)).ToArray();

            var parametersToAppendInsteadOfChange = new List<JToken>();
            ProcessRemove(parametersRoot, optionParameters);
            ChangeParameters(parametersRoot, optionParameters, parametersToAppendInsteadOfChange);

            ProcessPackages(parametersRoot, optionParameters);
            ProcessTarifficationSteps(parametersRoot, optionParameters);
            ProcessAppend((JArray)parametersRoot, optionParameters, parametersToAppendInsteadOfChange);
        }

        private void ProcessRemove(JToken parameters, JToken[] optionParameters)
        {
            var removeParameters =
                optionParameters.Where(
                    n => n.SelectTokens("BaseParameterModifiers.[?(@.Alias)].Alias").Any(m => m.ToString() == "Remove")).ToArray();

            foreach (var param in removeParameters)
            {
                foreach (var jToken in FindByKey(parameters, param.ExtractDirection().GetKey()))
                {
                    jToken.Remove();
                }
            }
        }

        private void ProcessAppend(JArray parameters, IEnumerable<JToken> optionParameters, List<JToken> parametersToAppendInsteadOfChange)
        {
            foreach (var param in parametersToAppendInsteadOfChange)
            {
                parameters.Add(param.DeepClone().SetChanged());
            }

            var addParameters =
                optionParameters.Where(
                    n => n.SelectTokens("BaseParameterModifiers.[?(@.Alias)].Alias").Any(m => m.ToString() == "Append")).ToArray();

            foreach (var param in addParameters)
            {
                parameters.Add(param.DeepClone().SetChanged());
            }
        }

        private void ChangeParameters(JToken parametersRoot, IEnumerable<JToken> optionParameters, List<JToken> parametersToAdd)
        {
            foreach (var optionParam in optionParameters.Where(n => !n.SelectTokens("BaseParameterModifiers.[?(@.Alias)].Alias").Any(m => TariffDirection.SpecialModifiers.Contains(m.ToString()))))
            {
                var key = optionParam.ExtractDirection().GetKey();
                var parametersToProcess = FindByKey(parametersRoot, key).ToArray();
                var modifiers = new HashSet<string>(optionParam.SelectTokens("Modifiers.[?(@.Alias)].Alias").Select(n => n.ToString()));
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
                            param["NumValue"] = (decimal)param["NumValue"] *
                                                (1 - (decimal)optionParam["NumValue"]);
                            param.SetChanged();

                        }
                        else
                        {
                            if (!skipProcessing)
                            {
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
                if (c.Count >= 2)
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

        private void MergeLinkWithOption(JArray optionParameters, IEnumerable<JToken> linkParameters)
        {
            foreach (var linkParameter in linkParameters)
            {
                bool processed = false;
                if (linkParameter["BaseParameter"] != null)
                {
                    var key = linkParameter.ExtractDirection().GetKey(false);
                    var parametersToProcess = FindByKey(optionParameters, key, false).ToArray();
                    processed = parametersToProcess.Any();
                    foreach (var p in parametersToProcess)
                    {
                        p.Replace(linkParameter.DeepClone());
                    }
                }

                if (!processed)
                {
                    optionParameters.Add(linkParameter);
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