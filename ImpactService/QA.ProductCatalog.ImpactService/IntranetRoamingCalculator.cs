using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.ImpactService
{
    public class IntranetRoamingCalculator : BaseImpactCalculator
    {

        public bool UseTariffData {get; private set; }

        public bool MergeWithTariffData { get; private set; }


        public bool ExcludePartners { get; private set; }

        public IntranetRoamingCalculator() : base("UseForRoamingCalculator", "CalculateInRoaming", "ServicesOnTariff")
        {

        }

        public void MergeLinkImpactToRoamingScale(JObject scale, JObject product, bool useMacroRegionParameters)
        {
            int scaleId = (int) scale["Id"];

            var link = product
                .SelectTokens($"RoamingScalesOnTariff.[?(@.RoamingScale)]")
                .FirstOrDefault(n => (int)n.SelectToken("RoamingScale.Id") == scaleId)
                ?.SelectToken("Parent");

            if (link == null)
                return;

            var modifiersRoot = link.SelectToken("Modifiers");
            var modifiersSeq = modifiersRoot?.SelectTokens("[?(@.Alias)].Alias")?.Select(n => n.ToString()) ??
                               Enumerable.Empty<string>();
            var modifiers = new HashSet<string>(modifiersSeq);

            UseTariffData = modifiers.Contains("UseTariffData");
            MergeWithTariffData = modifiers.Contains("MergeWithTariffData");
            ExcludePartners = modifiers.Contains("ExcludePartners");
            var scaleParameters = (JArray)scale.SelectToken("Parameters");

            if (MergeWithTariffData)
            {
                var tariffParameters = (JArray)product.SelectToken("Parameters");
                MergeTariffParametersToScale(tariffParameters, scaleParameters);
            }

            var scaleLinkParameters = (JArray)link.SelectToken("Parameters");

            if (scaleLinkParameters == null) return;

            if (!UseTariffData)
            {
                MergeLinkImpact(scaleParameters, scaleLinkParameters, useMacroRegionParameters ? "ForHomeMacroRegion" : null);
                ProcessRemoveModifier(scaleParameters);
            }
            else
            {
                var tariffParameters = (JArray)product.SelectToken("Parameters");
                MergeLinkImpact(tariffParameters, scaleLinkParameters);
            }

        }

        private void MergeTariffParametersToScale(JArray tariffParameters, JArray scaleParameters)
        {
            var parameters = FilterTariffParameters(tariffParameters);
            scaleParameters.Add(parameters);
        }


        public IEnumerable<JToken> FilterTariffParameters(JArray root)
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
            var defaultScale = scales.FirstOrDefault(n => n.SelectToken("MarketingProduct.Regions") == null);
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

        public JArray GetResultParameters(JObject scale, JObject product, string region, bool useMacroRegionParameters)
        {
            MergeLinkImpactToRoamingScale(scale, product, useMacroRegionParameters);

            IEnumerable<JToken> parameters = (!UseTariffData)
                ? FilterScaleParameters((JArray)scale.SelectToken("Parameters"))
                : FilterTariffParameters((JArray)product.SelectToken("Parameters"));
            
            return new JArray(region == null ? parameters : FilterResultParameters(parameters, region));
        }
        
        public IEnumerable<JToken> FilterResultParameters(IEnumerable<JToken> parameters, string region)
        {
            return parameters == null ? new JToken[] {} : FilterResultParametersInternal(parameters, region);
        }
        
        private IEnumerable<JToken> FilterResultParametersInternal(IEnumerable<JToken> parameters, string region)
        {
            foreach (var p in parameters)
            {
                if (p.SelectToken("Zone.Regions") != null)
                {
                    var aliases = new HashSet<string>(p.SelectTokens("Zone.Regions.[?(@.Alias)].Alias").Select(n => n.ToString()));
                    if (!aliases.Contains(region)) continue;
                }

                yield return p;
            }
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

        public void FilterServiceParameters(JObject service, string region)
        {
            IEnumerable<JToken> parameters = (JArray) service.SelectToken("Parameters");
            service["Parameters"] = new JArray(FilterResultParameters(parameters, region));
        }
        
        public void FilterServiceOnTariffParameters(JObject product, string region)
        {
            var links = product.SelectTokens("ServicesOnTariff.[?(@.Service)]");

            foreach (var link in links)
            {
                var matrixElem = link.SelectToken("Parent");
                if (matrixElem == null) continue;
                IEnumerable<JToken> parameters = (JArray) matrixElem.SelectToken("Parameters");
                matrixElem["Parameters"] = new JArray(FilterResultParameters(parameters, region));
            }
        }
    }
}
