using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
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

        public void WriteExtraNodes(JsonNode input, JsonArray extraNodes, ProductsOptionsExpand options)
        {
            if (!extraNodes.Any())
            {
                return;
            }

            var extraNodesDict = extraNodes
                .Select(n => n.AsObject())
                .ToDictionary(_productReadExpandPostProcessor.GetId);

            foreach (var extraNode in extraNodesDict.Values)
            {
                extraNodes.Remove(extraNode);
            }

            if (input is JsonArray)
            {
                foreach (var item in input.AsArray())
                {
                    WriteExpand(item.AsObject(), extraNodesDict, options);
                }
            }
            else
            {
                WriteExpand(input.AsObject(), extraNodesDict, options);
            }
        }

        private void WriteExpand(JsonObject expandableNode, Dictionary<int, JsonObject> extraNodesDict, ProductsOptionsExpand options)
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

        private void WriteExpandInPlace(JsonObject expandableNode, Dictionary<int, JsonObject> extraNodesDict,
            ProductsOptionsExpand options)
        {
            var nodeList = PostProcessHelper.Select(expandableNode, options.Path);
            foreach (var node in nodeList)
            {
                if (node is JsonArray)
                {
                    TryReplaceArray(node.AsArray(), extraNodesDict);
                }
                else if (node is JsonObject)
                {
                    TryReplaceNode(node.AsObject(), extraNodesDict);
                }
            }
        }

        private string GetPropertyName(JsonNode node)
        {
            var path = node.GetPath().Replace(node.Parent.GetPath(), "");
            var result = Regex.Replace(path, "[$\"\\['\\]\\.]", "");
            return result;
        }
        
        private int GetIndex(JsonNode node)
        {
            return int.Parse(GetPropertyName(node));
        }

        private void TryReplaceNode(JsonObject node, Dictionary<int, JsonObject> extraNodesDict)
        {
            if (extraNodesDict.TryGetValue(_productReadExpandPostProcessor.GetId(node), out var value))
            {
                ReplaceNode(node, PostProcessHelper.CloneJsonNode(value));
            }
        }
        
        private void TryReplaceArray(JsonArray array, Dictionary<int, JsonObject> extraNodesDict)
        {
            var resultArr = new JsonArray();
            foreach (var item in array)
            {
                var result = item;
                if (extraNodesDict.TryGetValue(_productReadExpandPostProcessor.GetId(item.AsObject()), out var value))
                {
                    result = value;
                }
                resultArr.Add(PostProcessHelper.CloneJsonNode(result));
            }
            ReplaceNode(array, resultArr);
        }

        private void ReplaceNode(JsonNode oldNode, JsonNode newNode)
        {
            var parent = oldNode.Parent;
            if (parent != null)
            {
                if (parent is JsonObject)
                {
                    parent[GetPropertyName(oldNode)] = newNode;
                }
                else if (parent is JsonArray)
                {
                    parent[GetIndex(oldNode)] = newNode;
                }
            }   
        }
        

        private void WriteExpandInField(JsonObject expandableNode, Dictionary<int, JsonObject> extraNodesDict, ProductsOptionsExpand options)
        {
            var ids = _productReadExpandPostProcessor.GetExpandIds(expandableNode, options);

            var values = ids
                .Where(id => extraNodesDict.ContainsKey(id))
                .Select(id => PostProcessHelper.CloneJsonNode(extraNodesDict[id]))
                .ToArray();

            expandableNode[options.Name] = new JsonArray(values);
        }

    }
}
