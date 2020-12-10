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
            "WithinPackage", "OverPackage", "FirstStep", "SecondStep", "ThirdStep", "FourthStep", "FifthStep"
        });

        public TariffDirection(string baseParameter, string zone, string direction, string[] baseParameterModifiers)
        {
            BaseParameter = baseParameter;
            Zone = zone;
            Direction = direction;
            BaseParameterModifiers = baseParameterModifiers?.OrderBy(n => n).ToArray();
        }

        public static string FeeKey = new TariffDirection("SubscriptionFee", null, null, null).GetKey();

        public static string FederalFeeKey = new TariffDirection("SubscriptionFee", null, null, new[] { "Federal" }).GetKey();

        public static string CityFeeKey = new TariffDirection("SubscriptionFee", null, null, new[] { "City" }).GetKey();

        public string GetKey(bool excludeSpecial = false, bool excludeZone = false)
        {
            return GetKey(new DirectionExclusion((excludeSpecial) ? SpecialModifiers : null) {Zone = excludeZone});
        }

        public string GetKey(DirectionExclusion exclusion)
        {
            var sb = new StringBuilder();
            if (BaseParameter != null)
            {
                sb.Append($"BaseParameter: {BaseParameter}; ");
                var zone = (exclusion.Zone) ? string.Empty : (Zone ?? string.Empty);
                sb.Append($"Zone: {zone}; ");
                var direction = (exclusion.Direction) ? string.Empty : (Direction ?? string.Empty);
                sb.Append($"Direction: {direction}; ");
                var modifiers = BaseParameterModifiers ?? Enumerable.Empty<string>();
                if (exclusion.Modifiers != null)
                    modifiers = modifiers.Where(n => !exclusion.Modifiers.Contains(n));
                sb.Append($"BaseParameterModifiers: {string.Join(",", modifiers)};");

            }
            return sb.ToString();

        }

    }
}