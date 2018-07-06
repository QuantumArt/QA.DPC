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

        public JObject Calculate(JObject option, string countryCode)
        {
            FilterByCountryCode(option, countryCode);
            SetUnlimitedValues(option);
            return option;
        }

        public JObject Calculate(JObject roamingScale, JObject[] options, string countryCode, string homeRegion)
        {
            FilterByCountryCode(roamingScale, countryCode);
            foreach (var option in options)
            {
                FilterByCountryCode(option, countryCode);
                SetUnlimitedValues(option);
                Calculate(roamingScale, option);
            }
            Reorder(roamingScale, homeRegion);
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

            if (!countryParams.Any())
            {
                foreach (var zp in zoneParameters)
                {
                    var codes = new HashSet<string>(zp.SelectTokens("Zone.RoamingCountries.[?(@.Country)].Country.Code").Select(n => n.ToString()));
                    var aliases = new HashSet<string>(zp.SelectTokens("Zone.RoamingCountries.[?(@Alias)].Alias").Select(n => n.ToString()));
                    if (codes.Contains(countryCode) || aliases.Contains(countryCode))
                    {
                        countryParams.Add((int)zp["Id"], zp);
                    }
                }
            }

            var preservedParams = new Dictionary<int, JToken>(countryParams);

            var exclusion = new DirectionExclusion(new[] {"Unlimited"}) { Zone = true };

            foreach (var p in worldExceptRussiaParams)
            {
                var key = p.ExtractDirection().GetKey(exclusion);
                var specialExists = countryParams.Values.Any(n => n.ExtractDirection().GetKey(exclusion) == key);
                if (!specialExists)
                {
                    preservedParams.Add((int)p["Id"], p);
                }
            }

            foreach (var zp in zoneParameters)
            {
                if (!preservedParams.ContainsKey((int) zp["Id"]))
                {
                    zp.Remove();
                }
                else
                {
                    zp["Zone"] = null;
                }
            }
        }
    }
}