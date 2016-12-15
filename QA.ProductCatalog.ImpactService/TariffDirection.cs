using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QA.ProductCatalog.ImpactService
{
    public class TariffDirection
    {

        public string BaseParameter { get; }

        public string Zone { get; set; }

        public string Direction { get; set; }

        public string[] BaseParameterModifiers { get; }

        public static HashSet<string> SpecialModifiers { get; } = new HashSet<string>(new[]
        {
            "WithinPackage", "OverPackage", "FirstStep", "SecondStep", "ThirdStep", "FourthStep", "FifthStep",
            "ZoneExpansion", "Unlimited"
        });

        public TariffDirection(string baseParameter, string zone, string direction, string[] baseParameterModifiers)
        {
            BaseParameter = baseParameter;
            Zone = zone;
            Direction = direction;
            BaseParameterModifiers = baseParameterModifiers?.OrderBy(n => n).ToArray();
        }

        public string GetKey(bool excludeSpecial = true, bool excludeZone = false)
        {
            var sb = new StringBuilder();
            if (BaseParameter != null)
            {
                sb.Append($"BaseParameter: {BaseParameter}; ");
                var zone = (excludeZone) ? String.Empty : (Zone ?? string.Empty);
                sb.Append($"Zone: {zone}; ");
                sb.Append($"Direction: {Direction ?? string.Empty}; ");
                IEnumerable<string> modifiers = BaseParameterModifiers ?? Enumerable.Empty<string>();
                if (excludeSpecial)
                    modifiers = modifiers.Where(n => !SpecialModifiers.Contains(n));
                sb.Append($"BaseParameterModifiers: {string.Join(",", modifiers)};");

            }
            return sb.ToString();

        }


    }
}