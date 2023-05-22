using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Exceptions;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.PostProcessing
{
    public class ExpandReadProcessor : IProductReadExpandPostProcessor
    {
        public int[] GetExpandIdsWithVerification(JToken input, ProductsOptionsExpand options)
        {
            var ids = new HashSet<int>();

            if (input is JArray)
            {
                foreach (var item in input)
                {
                    VerifyAndCollectIds(ids, item, options);
                }
            }
            else
            {
                VerifyAndCollectIds(ids, input, options);
            }

            return ids.ToArray();
        }

        public int[] GetExpandIds(JToken expandableNode, ProductsOptionsExpand options)
        {
            var ids = new HashSet<int>();

            foreach (var token in expandableNode.SelectTokens(options.Path))
            {
                if (token is JArray)
                {
                    foreach (var item in token)
                    {
                        ids.Add(GetId(item));
                    }
                }
                else if (token is JObject)
                {
                    ids.Add(GetId(token));
                }
            }

            return ids.ToArray();
        }

        public int GetId(JToken token)
        {
            return GetIdToken(token).Value<int>();
        }

        private void VerifyAndCollectIds(HashSet<int> ids, JToken expandableNode, ProductsOptionsExpand options)
        {
            if (options.Name != null)
            {
                EnsureUnusedProperty(expandableNode, options.Name);
            }

            CollectIdsSafe(ids, expandableNode, options);
        }

        private void CollectIdsSafe(HashSet<int> ids, JToken expandableNode, ProductsOptionsExpand options)
        {
            foreach (var token in expandableNode.SelectTokens(options.Path))
            {
                if (token is JArray)
                {
                    foreach (var item in token)
                    {
                        TryAddId(ids, item);
                    }
                }
                else if (token is JObject)
                {
                    TryAddId(ids, token);
                }
            }
        }

        private JToken GetIdToken(JToken token) => token[HighloadConstants.IdField];

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

        private void TryAddId(HashSet<int> ids, JToken token)
        {
            var idToken = GetIdToken(token);
            if (idToken == null)
            {
                throw new MissingIdExpandException(
                    $"Unable to expand data because expandable object {token.Path} doesn't contain '{HighloadConstants.IdField}' field required for this operation",
                    $"Не удалось дозагрузить данные, т.к. объект источника {token.Path} не содержит свойство '{HighloadConstants.IdField}', необходимое для данной операции");
            }

            ids.Add(idToken.Value<int>());
        }
    }
}
