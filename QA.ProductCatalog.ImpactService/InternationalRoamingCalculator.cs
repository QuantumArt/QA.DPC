using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.ImpactService
{
    public class InternationalRoamingCalculator : BaseImpactCalculator
    {
        public InternationalRoamingCalculator() : base("UseForRoamingCalculator", "CalculateInRoaming", "ServicesOnRoamingScale")
        {

        }

        public JObject Calculate(JObject roamingScale, JObject[] options, string countryCode)
        {
            foreach (var option in options)
            {
                FilterByCountryCode(option, countryCode);
                SetUnlimitedValues(option);
                Calculate(roamingScale, option);
            }
            Reorder(roamingScale);
            return roamingScale;
        }

        private void SetUnlimitedValues(JObject option)
        {
            var parameters = option["Parameters"];
            if (parameters == null) return;
            foreach (var param in parameters)
            {
                var title = param.SelectToken("BaseParameterModifiers[?(@.Alias == 'Unlimited')].Title")?.ToString();
                if (title != null)
                {
                    param["Value"] = title.ToLowerInvariant();
                }
            }
        }

        private void FilterByCountryCode(JObject option, string countryCode)
        {

            var zoneParameters = option.SelectTokens("Parameters.[?(@.Zone)]").ToArray();

            var worldExceptRussiaParams = zoneParameters.Where(n => n.SelectToken("Zone.Alias").ToString() == "WorldExceptRussia").ToArray();

            var countryParams = zoneParameters.Where(n => n.SelectToken("Zone.Alias").ToString() == countryCode)
                .ToDictionary(k => (int)k["Id"], p => p);

            var preservedParams = new Dictionary<int, JToken>(countryParams);

            foreach (var p in worldExceptRussiaParams)
            {
                var dir = p.ExtractDirection();
                dir.Zone = countryCode;
                var specialExists = FindByKey(option.SelectToken("Parameters"), dir.GetKey(), new DirectionExclusion(new [] {"Unlimited"})).Any();
                if (!specialExists)
                    preservedParams.Add((int)p["Id"], p);
            }

            foreach (var zp in zoneParameters)
            {
                if (!preservedParams.ContainsKey((int) zp["Id"]))
                {
                    zp.Remove();
                }
            }
        }
    }
}