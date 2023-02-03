using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.TmForum.Extensions
{
    internal static class JObjectExtension
    {
        private static readonly ICollection<string> _notUpdatableFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "id",
            "href",
            "lastUpdate",
            "@type",
            "@baseType"
        };

        internal static JObject RemoveUpdateRestrictedFields(this JObject json)
        {
            RemoveRestricted(json);

            return json;
        }

        private static void RemoveRestricted(JObject json)
        {
            JProperty[] properties = json.Properties().ToArray();

            foreach (var property in properties)
            {
                switch (property.Value.Type)
                {
                    case JTokenType.Object:
                        RemoveRestricted((JObject)property.Value);
                        break;
                    case JTokenType.Array:
                        break;
                    default:
                        if (_notUpdatableFields.Contains(property.Name))
                        {
                            property.Remove();
                        }
                        break;
                }
            }
        }
    }
}
