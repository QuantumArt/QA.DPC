using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.ImpactService
{
    public class TariffOptionCalculator : BaseImpactCalculator
    {
        public TariffOptionCalculator() : base("UseForCalculator", "Calculate", "ServicesOnTariff", false)
        {

        }
    }
}
