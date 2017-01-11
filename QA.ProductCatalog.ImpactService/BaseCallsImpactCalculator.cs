using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.ImpactService
{
    public abstract class BaseCallsImpactCalculator : BaseImpactCalculator
    {
        protected BaseCallsImpactCalculator(string parameterModifierName, string linkModifierName, string linkName, bool restrictedImpact) : base(parameterModifierName, linkModifierName, linkName, restrictedImpact)
        {
        }

        public abstract IEnumerable<JToken> FilterProductParameters(JArray root, string code);

        public virtual void FilterServicesParameters(JObject[] services, string region)
        {
            foreach (var service in services)
            {
                var root = (JArray)service.SelectToken("Parameters");
                var countryParams = FilterProductParameters(root, region);
                service["Parameters"] = new JArray(countryParams);
            }
        }
    }
}
