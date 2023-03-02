using Newtonsoft.Json.Linq;
using QA.ProductCatalog.TmForum.Models;

namespace QA.ProductCatalog.TmForum.Extensions
{
    internal static class JObjectExtension
    {
        internal static JObject RemoveUpdateRestrictedFields(this JObject json)
        {
            RemoveRestricted(json);

            return json;
        }

        private static void RemoveRestricted(JObject json)
        {
            JProperty[] properties = json.Properties().ToArray();

            foreach (JProperty property in properties)
            {
                switch (property.Value.Type)
                {
                    case JTokenType.Object:
                        RemoveRestricted((JObject)property.Value);
                        break;
                    case JTokenType.Array:
                        break;
                    default:
                        if (InternalTmfSettings.NotUpdatableFields.Contains(property.Name))
                        {
                            property.Remove();
                        }
                        break;
                }
            }
        }
    }
}
