using System.Collections.Generic;

namespace QA.ProductCatalog.ImpactService
{
    public class DirectionExclusion
    {

        public DirectionExclusion(IEnumerable<string> excludeModifiers)
        {
            Modifiers = excludeModifiers != null ? new HashSet<string>(excludeModifiers) : new HashSet<string>();
        }

        public bool Zone { get; set; }

        public bool Direction { get; set; }

        public HashSet<string> Modifiers { get; set; }
    }
}