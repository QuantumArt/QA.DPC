using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.PostProcessing
{
    public class ExpandWriteProcessor : ExpandBaseProcessor, IProductWriteExpandPostProcessor
    {
        public void WriteExtraNodes(JToken input, JArray extraNodes, ProductsOptionsExpand options)
        {
            if (!extraNodes.Any())
            {
                return;
            }

            var extraNodesDict = extraNodes.ToDictionary(GetId);

            if (input is JArray)
            {
                foreach (var item in input.ToArray())
                {
                    Expand(item, extraNodesDict, options);
                }
            }
            else
            {
                Expand(input, extraNodesDict, options);
            }
        }

        private void Expand(JToken expandableNode, Dictionary<int, JToken> extraNodesDict, ProductsOptionsExpand options)
        {
            if (options.Name == null)
            {
                ExpandInPlace(expandableNode, extraNodesDict, options);
            }
            else
            {
                ExpandInField(expandableNode, extraNodesDict, options);
            }
        }

        private void ExpandInPlace(JToken expandableNode, Dictionary<int, JToken> extraNodesDict, ProductsOptionsExpand options)
        {
            foreach (var token in expandableNode.SelectTokens(options.Path).ToArray())
            {
                if (token is JArray)
                {
                    foreach (var item in token.ToArray())
                    {
                        TryReplaceNode(item, extraNodesDict);
                    }
                }
                else if (token is JObject)
                {
                    TryReplaceNode(token, extraNodesDict);
                }
            }
        }

        private void TryReplaceNode(JToken node, Dictionary<int, JToken> extraNodesDict)
        {
            if (extraNodesDict.TryGetValue(GetId(node), out var value))
            {
                node.Replace(value);
            }
        }

        private void ExpandInField(JToken expandableNode, Dictionary<int, JToken> extraNodesDict, ProductsOptionsExpand options)
        {
            var ids = new HashSet<int>();
            CollectIds(ids, expandableNode, options);

            var values = ids
                .Where(id => extraNodesDict.ContainsKey(id))
                .Select(id => extraNodesDict[id])
                .ToArray();
            expandableNode[options.Name] = new JArray(values);
        }
    }
}
