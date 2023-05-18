using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Exceptions;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.PostProcessing
{
    public class ExpandReadProcessor : ExpandBaseProcessor, IProductReadExpandPostProcessor
    {
        public void EnsureExpandIsPossible(JToken input, ProductsOptionsExpand expandOptions)
        {
            if (expandOptions.Name == null)
            {
                return;
            }

            if (input is JArray)
            {
                foreach (var item in input.ToArray())
                {
                    EnsureUnusedProperty(item, expandOptions.Name);
                }
            }
            else
            {
                EnsureUnusedProperty(input, expandOptions.Name);
            }
        }

        public int[] GetExpandIds(JToken input, ProductsOptionsExpand options)
        {
            var ids = new HashSet<int>();

            if (input is JArray)
            {
                foreach (var item in input)
                {
                    CollectIds(ids, item, options);
                }
            }
            else
            {
                CollectIds(ids, input, options);
            }

            return ids.ToArray();
        }

        private void EnsureUnusedProperty(JToken expandableNode, string propertyName)
        {
            var usedProperties = expandableNode.Children()
                .Select(x => ((JProperty)x).Name)
                .ToArray();
            if (usedProperties.Contains(propertyName))
            {
                throw new NamedPropertyBusyExpandException(
                    $"Unable to expand data to named property '{propertyName}' because it is already in use in expandable object",
                    $"Не удалось дозагрузить данные в именованное свойство '{propertyName}', т.к. оно уже используется в целевом объекте");
            }
        }
    }
}
