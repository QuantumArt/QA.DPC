using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.ImpactService
{
    public class InternationalCallsCalclulator : BaseCallsImpactCalculator
    {
        public InternationalCallsCalclulator()
            : base("UseForInternationalCallsCalculator", "CalculateInInternationalCalls", "ServicesOnTariff", true)
        {

        }

        public override IEnumerable<JToken> FilterProductParameters(JArray root, string countryCode)
        {

            var markedParams = root
                .Where(
                    n =>
                        n.SelectTokens("Modifiers.[?(@.Alias)].Alias")
                            .Select(m => m.ToString())
                            .Contains(ParameterModifierName)).ToArray();
            var countryParams = markedParams
                .Where(n => n.SelectTokens("Direction.Countries.[?(@.Code)].Code").Select(m => m.ToString()).Contains(countryCode)).ToArray();

            if (countryParams.Length == 0)
            {
                countryParams =
                    markedParams.Where(n => n.SelectToken("Direction.Alias")?.ToString() == "OtherCountries").ToArray();

            }
            else if (countryParams.Length > 1)
            {
                var dirCount = new Dictionary<string, JToken>();
                var toDelete = new List<JToken>();
                foreach (var countryParam in countryParams)
                {
                    var key = countryParam.ExtractDirection().GetKey();

                    if (!dirCount.ContainsKey(key))
                    {
                        dirCount.Add(key, countryParam);
                    }
                    else
                    {
                        var saved = dirCount[key];
                        if (CountDirectionCountries(countryParam) < CountDirectionCountries(saved))
                        {
                            toDelete.Add(saved);
                            dirCount[key] = countryParam;
                        }
                    }
                }

                foreach (var p in toDelete)
                {
                    p.Remove();
                }
            }

            foreach (var p in countryParams)
            {
                p["Title"] = GenerateNewTitle(p);
                p["SpecialDirection"] = true;
            }

            countryParams = countryParams.Union(markedParams.Where(n => n["Direction"] == null)).ToArray();

            return countryParams;
        }

        private static string GenerateNewTitle(JToken p)
        {
            var newTitle = p["Title"].ToString();
            var roamingTitle = p["TitleForIcin"]?.ToString();
            var pos = newTitle.IndexOf(" (", StringComparison.InvariantCulture);
            var bpTitle = p.SelectToken("BaseParameter.Title").ToString();
            newTitle = roamingTitle ?? ((pos == -1) ? bpTitle : bpTitle + newTitle.Substring(pos));
            return newTitle;
        }

        private static int CountDirectionCountries(JToken countryParam)
        {
            return countryParam.SelectTokens("Direction.Countries").Count();
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
