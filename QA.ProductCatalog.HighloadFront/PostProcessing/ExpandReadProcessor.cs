using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.Path;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Constants;
using QA.ProductCatalog.HighloadFront.Exceptions;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.PostProcessing
{
    public class ExpandReadProcessor : IProductReadExpandPostProcessor
    {
        public int[] GetExpandIdsWithVerification(JsonNode input, ProductsOptionsExpand options)
        {
            var ids = new HashSet<int>();

            if (input is JsonArray)
            {
                foreach (var item in input.AsArray())
                {
                    VerifyAndCollectIds(ids, item.AsObject(), options);
                }
            }
            else
            {
                VerifyAndCollectIds(ids, input.AsObject(), options);
            }

            return ids.ToArray();
        }

        public int[] GetExpandIds(JsonObject expandableNode, ProductsOptionsExpand options)
        {
            var ids = new HashSet<int>();

            var nodeList = PostProcessHelper.Select(expandableNode, options.Path);
            foreach (var node in nodeList)
            {
                if (node is JsonArray)
                {
                    foreach (var item in node.AsArray())
                    {
                        ids.Add(GetId(item.AsObject()));
                    }
                }
                else if (node is JsonObject)
                {
                    ids.Add(GetId(node.AsObject()));
                }
            }
            return ids.ToArray();
        }

        public int GetId(JsonObject token)
        {
            return GetIdToken(token).GetValue<int>();
        }

        private void VerifyAndCollectIds(HashSet<int> ids, JsonObject expandableNode, ProductsOptionsExpand options)
        {
            if (options.Name != null)
            {
                EnsureUnusedProperty(expandableNode, options.Name);
            }

            CollectIdsSafe(ids, expandableNode, options);
        }

        private void CollectIdsSafe(HashSet<int> ids, JsonNode expandableNode, ProductsOptionsExpand options)
        {
            var nodeList = PostProcessHelper.Select(expandableNode, options.Path);
            foreach (var node in nodeList)
            {
                if (node is JsonArray)
                {
                    foreach (var item in node.AsArray())
                    {
                        TryAddId(ids, item.AsObject());
                    }
                }
                else if (node is JsonObject)
                {
                    TryAddId(ids, node.AsObject());
                }
            }
        }

        private JsonNode GetIdToken(JsonObject token)
        {
            return token[HighloadFields.Id].AsValue();
        }

        private void EnsureUnusedProperty(JsonObject expandableNode, string propertyName)
        {
            var usedProperties = expandableNode
                .Select(x => x.Key)
                .ToArray();
            
            if (usedProperties.Contains(propertyName))
            {
                throw new NamedPropertyBusyExpandException(
                    $"Unable to expand data to named property '{propertyName}' because it is already in use in expandable object",
                    $"Не удалось дозагрузить данные в именованное свойство '{propertyName}', т.к. оно уже используется в целевом объекте");
            }
        }

        private void TryAddId(HashSet<int> ids, JsonObject token)
        {
            var idToken = GetIdToken(token);
            if (idToken == null)
            {
                throw new MissingIdExpandException(
                    $"Unable to expand data because expandable object {token} doesn't contain '{HighloadFields.Id}' field required for this operation",
                    $"Не удалось дозагрузить данные, т.к. объект источника {token} не содержит свойство '{HighloadFields.Id}', необходимое для данной операции");
            }

            ids.Add(idToken.GetValue<int>());
        }
    }
}
