using System.Linq;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.ImpactService
{
    public static class JTokenExtensions
    {
        public static TariffDirection ExtractDirection(this JToken parameter)
        {
            var result = new TariffDirection(
                parameter.SelectToken("BaseParameter.Alias")?.ToString(),
                parameter.SelectToken("Zone.Alias")?.ToString(),
                parameter.SelectToken("Direction.Alias")?.ToString(),
                parameter.SelectTokens("BaseParameterModifiers.[?(@.Alias)].Alias").Select(n => n.ToString()).ToArray()
            );
            return result;
        }

        public static JToken SetChanged(this JToken param)
        {
            param["Changed"] = true;
            return param;
        }
    }
}