using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.ImpactService
{
    public class IntranetRoamingCalculator : BaseImpactCalculator
    {

        public bool UseTariffData {get; private set; }

        public bool ExcludePartners { get; private set; }

        public IntranetRoamingCalculator() : base("UseForRoamingCalculator", "CalculateInRoaming", "ServicesOnTariff")
        {

        }

        public void MergeLinkImpactToRoamingScale(JObject scale, JObject product, string region)
        {
            int scaleId = (int) scale["Id"];

            var link = product
                .SelectTokens($"RoamingScalesOnTariff.[?(@.Id)]")
                .Single(n => (int)n.SelectToken("RoamingScale.Id") == scaleId)
                .SelectToken("Parent");

            var modifiersRoot = link.SelectToken("Modifiers");
            var modifiersSeq = modifiersRoot?.SelectTokens("[?(@.Alias)].Alias")?.Select(n => n.ToString()) ??
                               Enumerable.Empty<string>();
            var modifiers = new HashSet<string>(modifiersSeq);
            UseTariffData = modifiers.Contains("UseTariffData");
            ExcludePartners = modifiers.Contains("ExcludePartners");

            var scaleLinkParameters = (JArray)link.SelectToken("Parameters");

            if (scaleLinkParameters == null) return;

            if (!UseTariffData)
            {
                var scaleParameters = (JArray)scale.SelectToken("Parameters");


                MergeLinkImpact(scaleParameters, scaleLinkParameters);
                var toRemove =
                    scaleParameters.Where(
                            n => n.SelectTokens("Modifiers.[?(@.Alias)].Alias").Select(m => m.ToString()).Contains("Remove"))
                        .ToArray();

                foreach (var t in toRemove)
                {
                    t.Remove();
                }

            }
            else
            {
                var tariffParameters = (JArray)product.SelectToken("Parameters");
                MergeLinkImpact(tariffParameters, scaleLinkParameters);
            }

        }


        public IEnumerable<JToken> FilterProductParameters(JArray root)
        {
            var russiaParams =
                root.Where(n => 
                    new[] {"Russia", "RussiaExceptHome"}.Contains(n.SelectToken("Zone.Alias")?.ToString()) ||
                    n.SelectTokens("Modifiers.[?(@.Alias)].Alias").Select(m => m.ToString()).Contains(ParameterModifierName)
                ).ToArray();

            return AppendParents(root, russiaParams);
        }

        private IEnumerable<JToken> FilterScaleParameters(JArray root)
        {
            if (ExcludePartners)
            {
                return root.Where(
                    n =>
                        !n.SelectTokens("BaseParameterModifiers.[?(@.Alias)].Alias")
                            .Select(m => m.ToString())
                            .Contains("ThroughPartners")).ToArray();
            }
            else
            {
                return root.ToArray();
            }
        }

        public JObject FilterScale(string region, JObject[] scales)
        {
            var defaultScale = scales.SingleOrDefault(n => n.SelectToken("MarketingProduct.Regions") == null);
            foreach (var scale in scales)
            {
                var regionAliases =
                    scale.SelectTokens("MarketingProduct.Regions.[?(@.Region)].Region.Alias")
                        .Select(m => m.ToString())
                        .ToArray();

                if (!regionAliases.Contains(region)) continue;

                return scale;
            }

            return defaultScale;
           
        }

        public JArray GetResultParameters(JObject scale, JObject product, string region)
        {
            MergeLinkImpactToRoamingScale(scale, product, region);

            IEnumerable<JToken> parameters = (!UseTariffData)
                ? FilterScaleParameters((JArray)scale.SelectToken("Parameters"))
                : FilterProductParameters((JArray)product.SelectToken("Parameters"));

            var resultParameters = parameters as JArray ?? new JArray(parameters);
            return resultParameters;
        }

 

        public void MergeValuesFromTariff(JObject option, JArray tariffRoot)
        {
            foreach (var parameter in (JArray)option.SelectToken("Parameters"))
            {
                var modifiers = new HashSet<string>(
                    parameter.SelectTokens("Modifiers.[?(@.Alias)].Alias").Select(m => m.ToString())
                );

                if (!modifiers.Contains("UseValueFromTariff")) continue;

                var ex = new DirectionExclusion(null) { Zone = modifiers.Contains("ExcludeZoneWhileSearchingValue") };
                var searchResult = FindByKey(tariffRoot, parameter.ExtractDirection().GetKey(ex), ex).FirstOrDefault();

                if (searchResult == null) continue;

                if (searchResult["NumValue"] != null)
                {
                    parameter["NumValue"] = searchResult["NumValue"];
                }

                if (searchResult["Value"] != null)
                {
                    parameter["Value"] = searchResult["Value"];
                }
            }
        }
    }
}
