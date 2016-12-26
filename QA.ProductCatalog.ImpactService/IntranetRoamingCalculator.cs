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

        public JObject Calculate(JObject roamingScale, JObject[] options)
        {
            foreach (var option in options)
            {
                Calculate(roamingScale, option);
            }
            Reorder(roamingScale);
            return roamingScale;
        }
    }
}
