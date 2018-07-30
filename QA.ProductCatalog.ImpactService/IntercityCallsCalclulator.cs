using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.ImpactService
{
    public class IntercityCallsCalculator : BaseCallsImpactCalculator
    {
        public IntercityCallsCalculator(bool consolidateCallGroups = false)
             : base("UseForIntercityCallsCalculator", "CalculateInIntercityCalls", "ServicesOnTariff", consolidateCallGroups)
        {

        }

        public override IEnumerable<JToken> FilterProductParameters(JArray root, string region, bool generateNewTitles)
        {

            var markedParams = root
                .Where(
                    n =>
                        n.SelectTokens("Modifiers.[?(@.Alias)].Alias")
                            .Select(m => m.ToString())
                            .Contains(ParameterModifierName)).ToArray();
            var regionParams = markedParams
                .Where(n => n.SelectTokens("Direction.Regions.[?(@.Alias)].Alias").Select(m => m.ToString()).Contains(region)).ToArray();
            
            var toDelete = new HashSet<string>();

            if (regionParams.Length == 0)
            {
                regionParams =
                    markedParams.Where(n => n.SelectToken("Direction.Alias")?.ToString() == "Russia").ToArray();
            }

            else if (regionParams.Length > 1)
            {
                var dirCount = new Dictionary<string, JToken>();
                foreach (var regionParam in regionParams)
                {
                    var key = regionParam.ExtractDirection().GetKey(new DirectionExclusion() {Direction = true});

                    if (!dirCount.ContainsKey(key))
                    {
                        dirCount.Add(key, regionParam);
                    }
                    else
                    {
                        var saved = dirCount[key];
                        if (CountDirectionRegions(regionParam) < CountDirectionRegions(saved))
                        {
                            toDelete.Add(saved["Id"].ToString());
                            dirCount[key] = regionParam;
                        }
                        else
                        {
                            toDelete.Add(regionParam["Id"].ToString());
                        }
                    }
                }
            }
            
            if (toDelete.Any())
            {
                regionParams = regionParams.Where(n => !toDelete.Contains(n["Id"].ToString())).ToArray();
                markedParams = markedParams.Where(n => !toDelete.Contains(n["Id"].ToString())).ToArray();               
            }
            
            foreach (var p in regionParams)
            {
                if (generateNewTitles && p["Changed"] == null)
                {
                    p["Title"] = GenerateNewTitle(p);
                }
                p["SpecialDirection"] = true;
            }

            regionParams = regionParams.Union(markedParams.Where(n => n["Direction"] == null)).ToArray();

            regionParams = AppendParents(root, regionParams);

            if (ConsolidateCallGroups)
            {
                СonsolidateGroupForCalls(regionParams);
            }

            ChangeGroupNamesForIcin(regionParams);

            return regionParams;
        }

        protected override string GenerateNewTitle(JToken p)
        {
            var title = p["Title"].ToString();
            var roamingTitle = p["TitleForIcin"]?.ToString();
            return roamingTitle ?? title;
        }

        private static int CountDirectionRegions(JToken countryParam)
        {
            return countryParam.SelectTokens("Direction.Regions.[?(@.Alias)].Alias").Count();
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
                    if (c["NumValue"] != null && (int) c["NumValue"] >= (int) numValueToken)
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
