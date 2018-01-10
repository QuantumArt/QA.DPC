using System.Linq;

namespace QA.ProductCatalog.HighloadFront.Elastic.Extensions
{
    internal static class ConfigurationExtensions
    {
        public static string GetReindexUrlInternal(this IElasticConfiguration configuration, string language, string state)
        {
            var index = configuration.GetElasticIndices().FirstOrDefault(n => n.Language == language && n.State == state);

            if (index != null)
            {
                if (index.Date == null)
                {
                    return index.ReindexUrl;
                }
                else
                {
                    return $"{index.ReindexUrl}/{index.Date:s}";
                }
            }
            else
            {
                return null;
            }
        }
    }
}
