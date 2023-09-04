using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.PostProcessing
{
    public class ExpandWriteProcessor : IProductWriteExpandPostProcessor
    {
        private readonly IProductReadExpandPostProcessor _productReadExpandPostProcessor;

        public ExpandWriteProcessor(
            IProductReadExpandPostProcessor productReadExpandPostProcessor)
        {
            _productReadExpandPostProcessor = productReadExpandPostProcessor;
        }

        public void WriteExtraNodes(JToken input, JArray extraNodes, ProductsOptionsExpand options)
        {
            if (!extraNodes.Any())
            {
                return;
            }

            var extraNodesDict = extraNodes.ToDictionary(_productReadExpandPostProcessor.GetId);

            if (input is JArray)
            {
                foreach (var item in input.ToArray())
                {
                    WriteExpand(item, extraNodesDict, options);
                }
            }
            else
            {
                WriteExpand(input, extraNodesDict, options);
            }
        }

        private void WriteExpand(JToken expandableNode, Dictionary<int, JToken> extraNodesDict, ProductsOptionsExpand options)
        {
            if (options.Name == null)
            {
                WriteExpandInPlace(expandableNode, extraNodesDict, options);
            }
            else
            {
                WriteExpandInField(expandableNode, extraNodesDict, options);
            }
        }

        private void WriteExpandInPlace(JToken expandableNode, Dictionary<int, JToken> extraNodesDict, ProductsOptionsExpand options)
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
            if (extraNodesDict.TryGetValue(_productReadExpandPostProcessor.GetId(node), out var value))
            {
                node.Replace(value);
            }
        }

        private void WriteExpandInField(JToken expandableNode, Dictionary<int, JToken> extraNodesDict, ProductsOptionsExpand options)
        {
            var ids = _productReadExpandPostProcessor.GetExpandIds(expandableNode, options);

            var values = ids
                .Where(id => extraNodesDict.ContainsKey(id))
                .Select(id => extraNodesDict[id])
                .ToArray();

            expandableNode[options.Name] = new JArray(values);
        }
    }
}
