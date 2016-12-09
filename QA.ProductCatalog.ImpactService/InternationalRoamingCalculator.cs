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
            return roamingScale;
        }

        private void SetUnlimitedValues(JObject option)
        {
            foreach (var param in option["Parameters"])
            {
                if (param.SelectTokens("BaseParameterModifiers.Alias").Any(n => n.ToString() == "Unlimited"))
                {
                    param["Value"] = "безлимитный";
                }
            }
        }

        private void FilterByCountryCode(JObject option, string countryCode)
        {
            var optionParametersToRemove = option.SelectToken("Parameters").Where(n =>
            {
                var a = n.SelectTokens("Zone.RoamingCountries.Country.Code").ToArray();
                return !a.Any() || a.All(m => m.ToString() != countryCode);
            });

            foreach (var param in optionParametersToRemove)
            {
                param.Remove();
            }

        }
    }
}