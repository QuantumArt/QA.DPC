using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.ImpactService
{
    public class InternationalCallsCalclulator : BaseCallsImpactCalculator
    {
        public InternationalCallsCalclulator(bool consolidateCallGroups = false)
             : base("UseForInternationalCallsCalculator", "CalculateInInternationalCalls", "ServicesOnTariff", consolidateCallGroups)
        {
            
        }

        public override IEnumerable<JToken> FilterProductParameters(JArray root, string countryCode, bool generateNewTitles)
        {

            var markedParams = root
                .Where(
                    n =>
                        n.SelectTokens("Modifiers.[?(@.Alias)].Alias")
                            .Select(m => m.ToString())
                            .Contains(ParameterModifierName)).ToArray();
            var countryParams = markedParams
                .Where(n => n.SelectTokens("Direction.Countries.[?(@.Code)].Code").Select(m => m.ToString()).Contains(countryCode)).ToArray();

            var toDelete = new HashSet<string>();
            
            if (countryParams.Length == 0)
            {
                countryParams =
                    markedParams.Where(n => n.SelectToken("Direction.Alias")?.ToString() == "OtherCountries").ToArray();

            }
            else if (countryParams.Length > 1)
            {
                var dirCount = new Dictionary<string, JToken>();

                foreach (var countryParam in countryParams)
                {
                    var key = countryParam.ExtractDirection().GetKey(new DirectionExclusion() { Direction = true });

                    if (!dirCount.ContainsKey(key))
                    {
                        dirCount.Add(key, countryParam);
                    }
                    else
                    {
                        var saved = dirCount[key];
                        if (CountDirectionCountries(countryParam) < CountDirectionCountries(saved))
                        {
                            toDelete.Add(saved["Id"].ToString());
                            dirCount[key] = countryParam;
                        }
                        else
                        {
                            toDelete.Add(countryParam["Id"].ToString());
                        }
                    }
                }

            }

            if (toDelete.Any())
            {
                countryParams = countryParams.Where(n => !toDelete.Contains(n["Id"].ToString())).ToArray();
                markedParams = markedParams.Where(n => !toDelete.Contains(n["Id"].ToString())).ToArray();               
            }
          
            
            foreach (var p in countryParams)
            {
                if (generateNewTitles && p["New"] == null)
                {
                    p["Title"] = GenerateNewTitle(p);
                }
                p["SpecialDirection"] = true;
            }

            countryParams = countryParams.Union(markedParams.Where(n => n["Direction"] == null)).ToArray();

            countryParams = AppendParents(root, countryParams);

            if (ConsolidateCallGroups)
            {
                СonsolidateGroupForCalls(countryParams);
            }

            ChangeGroupNamesForIcin(countryParams);


            return countryParams;
        }

        private static int CountDirectionCountries(JToken countryParam)
        {
            return countryParam.SelectTokens("Direction.Countries.[?(@.Code)].Code").Count();
        }

        public override JObject Calculate(JObject tariff, JObject[] options, string homeRegion)
        {   
            var newTariff = base.Calculate(tariff, options, homeRegion);
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
                var numValueToken = p["NumValue"];
                var bpAlias = p.SelectToken("BaseParameter.Alias")?.ToString();
                if (numValueToken == null || bpAlias == null) continue;
                var ex = new DirectionExclusion() {Direction = true};
                var key = p.ExtractDirection().GetKey(ex);
                var toChange = FindByKey(tariffRoot, key, ex)
                    .Where(n => n["Changed"] == null && n["SpecialDirection"] != null).ToArray();

                foreach (var c in toChange)
                {
                    if (c["NumValue"] != null && (int) c["NumValue"] > (int) numValueToken)
                    {
                        if (c["OldNumValue"] == null)
                        {
                            c["OldNumValue"] = c["NumValue"];
                        }
                        c["NumValue"] = numValueToken;
                        c["Changed"] = true;
                    }
                }
            }
            return tariff;
        }
    }
}
