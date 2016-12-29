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
        public IntranetRoamingCalculator() : base("UseForRoamingCalculator", "CalculateInRoaming", "ServicesOnTariff")
        {

        }

        public bool MergeLinkImpactToRoamingScale(JObject scale, JObject product)
        {
            int scaleId = (int) scale["Id"];

            var link = product
                .SelectTokens($"RoamingScalesOnTariff.[?(@.Id)]")
                .Single(n => (int)n.SelectToken("RoamingScale.Id") == scaleId)
                .SelectToken("Parent");

            var modifiersRoot = link.SelectToken("Modifiers");

            var useTariffData = modifiersRoot != null && modifiersRoot.SelectTokens("[?(@.Alias)].Alias").Select(n => n.ToString()).ToArray().Contains("UseTariffData");

            if (!useTariffData)
            {
                var scaleLinkParameters = (JArray)link.SelectToken("Parameters");
                var scaleParameters = (JArray)scale.SelectToken("Parameters");


                MergeLinkImpact(scaleParameters, scaleLinkParameters);
                var toRemove =
                    scaleLinkParameters.Where(
                            n => n.SelectTokens("Modifiers.[?(@.Alias)].Alias").Select(m => m.ToString()).Contains("Remove"))
                        .ToArray();

                foreach (var t in toRemove)
                {
                    t.Remove();
                }

            }

            return useTariffData;
        }
    }
}
