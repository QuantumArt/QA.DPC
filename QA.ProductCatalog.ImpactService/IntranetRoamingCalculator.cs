using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.ImpactService
{
    public class IntranetRoamingCalculator : BaseImpactCalculator
    {
        public IntranetRoamingCalculator() : base("UseForRoamingCalculator", "CalculateInRoaming", "ServicesOnTariff", true)
        {

        }

        public bool MergeLinkImpactToRoamingScale(JObject scale, JObject product, string region)
        {
            int scaleId = (int) scale["Id"];

            var link = product
                .SelectTokens($"RoamingScalesOnTariff.[?(@.Id)]")
                .Single(n => (int)n.SelectToken("RoamingScale.Id") == scaleId)
                .SelectToken("Parent");

            var modifiersRoot = link.SelectToken("Modifiers");

            var useTariffDataInRegion = link.SelectToken($"UseTariffDataInRegions.[?(@.Alias == '{region}')]") != null;

            var useTariffData = useTariffDataInRegion || modifiersRoot != null && modifiersRoot.SelectTokens("[?(@.Alias)].Alias").Select(n => n.ToString()).ToArray().Contains("UseTariffData");

            var scaleLinkParameters = (JArray)link.SelectToken("Parameters");

            if (scaleLinkParameters == null) return useTariffData;

            if (!useTariffData)
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

            return useTariffData;
        }


        public IEnumerable<JToken> FilterForRoaming(JArray root)
        {
            var russiaParams =
                root.Where(n => 
                    new[] {"Russia", "RussiaExceptHome"}.Contains(n.SelectToken("Zone.Alias")?.ToString()) ||
                    n.SelectTokens("Modifiers.[?(@.Alias)].Alias").Select(m => m.ToString()).Contains("UseForRoamingCalculator")
                ).ToArray();

            return AppendParents(root, russiaParams);
        }

        public JObject FilterScale(string region, JObject[] scales)
        {
            return scales.SingleOrDefault(
                       n =>
                           n.SelectTokens("MarketingProduct.Regions.[?(@.Alias)].Alias")
                               .Select(m => m.ToString())
                               .Contains(region)) ??
                   scales.SingleOrDefault(n => n.SelectToken("MarketingProduct.Regions") == null);
        }

        public JArray GetResultParameters(JObject scale, JObject product, string region)
        {
            var useTariffData = MergeLinkImpactToRoamingScale(scale, product, region);

            IEnumerable<JToken> parameters = (!useTariffData)
                ? scale.SelectToken("Parameters")
                : FilterForRoaming((JArray)product.SelectToken("Parameters"));

            var resultParameters = parameters as JArray ?? new JArray(parameters);
            return resultParameters;
        }
    }
}
