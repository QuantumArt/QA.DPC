using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.ImpactService
{
    public class IntercityCallsCalculator : BaseImpactCalculator
    {
        public IntercityCallsCalculator()
            : base("UseForIntercityCallsCalculator", "CalculateInIntercityCalls", "ServicesOnTariff", true)
        {

        }

        public IEnumerable<JToken> FilterProductParameters(JArray root, int regionId)
        {

            var markedParams = root
                .Where(
                    n =>
                        n.SelectTokens("Modifiers.[?(@.Alias)].Alias")
                            .Select(m => m.ToString())
                            .Contains(ParameterModifierName)).ToArray();
            var regionParams = markedParams
                .Where(n => n.SelectTokens("Direction.Regions.[?(@.Id)].Id").Select(m => (int)m).Contains(regionId)).ToArray();

            if (regionParams.Length == 0)
            {
                regionParams =
                    markedParams.Where(n => n.SelectToken("Direction.Alias")?.ToString() == "Russia").ToArray();
            }

            else if (regionParams.Length > 1)
            {
                var dirCount = new Dictionary<string, JToken>();
                var toDelete = new List<JToken>();
                foreach (var regionParam in regionParams)
                {
                    var key = regionParam.ExtractDirection().GetKey();

                    if (!dirCount.ContainsKey(key))
                    {
                        dirCount.Add(key, regionParam);
                    }
                    else
                    {
                        var saved = dirCount[key];
                        if (CountDirectionRegions(regionParam) < CountDirectionRegions(saved))
                        {
                            toDelete.Add(saved);
                            dirCount[key] = regionParam;
                        }
                    }
                }

                foreach (var p in toDelete)
                {
                    p.Remove();
                }
            }

            foreach (var p in regionParams)
            {
                p["Title"] = GenerateNewTitle(p);
                p["SpecialDirection"] = true;
            }


            regionParams = regionParams.Union(markedParams.Where(n => n["Direction"] == null)).ToArray();

            return regionParams;
        }

        private static string GenerateNewTitle(JToken p)
        {
            var title = p["Title"].ToString();
            var roamingTitle = p["TitleForIcin"]?.ToString();
            return roamingTitle ?? title;
        }

        private static int CountDirectionRegions(JToken countryParam)
        {
            return countryParam.SelectTokens("Direction.Regions").Count();
        }

        public void FilterServicesParameters(JObject[] services, int regionId)
        {
            foreach (var service in services)
            {
                var root = (JArray)service.SelectToken("Parameters");
                var countryParams = FilterProductParameters(root, regionId);
                service["Parameters"] = new JArray(countryParams);
            }
        }

        public override JObject Calculate(JObject tariff, JObject[] options)
        {
            var newTariff = base.Calculate(tariff, options);
            foreach (var option in options)
            {
                newTariff = CalculateSpecialDirectionsImpact(newTariff, option);
            }
            return newTariff;
        }

        private JObject CalculateSpecialDirectionsImpact(JObject tariff, JObject option)
        {
            var tariffRoot = tariff.SelectToken("Parameters");
            var optionRoot = option.SelectToken("Parameters");
            foreach (var p in optionRoot.SelectTokens("[?(@.SpecialDirection == true)]"))
            {
                var bpAlias = p.SelectToken("BaseParameter.Alias")?.ToString();
                var numValueToken = p["NumValue"];
                if (numValueToken == null) continue;
                var toChange =
                    tariffRoot.SelectTokens($"[?(@.BaseParameter.Alias == '{bpAlias}')]")
                        .Where(n => n["Changed"] == null && n["SpecialDirection"] != null);

                foreach (var c in toChange)
                {
                    if (c["NumValue"] != null && (int) c["NumValue"] >= (int) numValueToken)
                    {
                        c["NumValue"] = numValueToken;
                        c["Changed"] = true;
                    }
                }
            }
            return tariff;
        }
    }
}
