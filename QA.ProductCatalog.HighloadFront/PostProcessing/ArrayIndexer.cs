using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Models;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.PostProcessing
{
    public class ArrayIndexer : IProductWritePostProcessor
    {
        private readonly ArrayIndexingSettings[] _arrayIndexingSettings;

        public ArrayIndexer(SonicElasticStoreOptions elasticOptions)
        {
            _arrayIndexingSettings = elasticOptions.IndexingOptions;
        }

        public void Process(ProductPostProcessorData data)
        {
            var product = data.Product;

            if (_arrayIndexingSettings == null)
                return;

            foreach (var setting in _arrayIndexingSettings)
            {
                var array = product[setting.Path];
                if (array == null)
                {
                    continue;
                }
                
                if (array is not JsonArray)
                {
                    throw new Exception($"Тип элемента по пути {setting.Path} должен быть массивом");                
                }
                var jArray = array.AsArray();
                
                var topKeyTokens = PostProcessHelper.Select(jArray, "[*]." + setting.Keys[0]);

                if (!topKeyTokens.Any())
                    continue;

                var objectFromArray = new JsonObject();
                foreach (var topKeyTokensWithArrayItem in topKeyTokens.GroupBy(x => NearestAncestorArrayElem(x)))
                {
                    var arrayItem = topKeyTokensWithArrayItem.Key;

                    string keyValue = GetKeyValue(topKeyTokensWithArrayItem, setting.KeyPartsSeparator);

                    if (string.IsNullOrEmpty(keyValue))
                        continue;

                    foreach (string keyPart in setting.Keys.Skip(1))
                    {
                        var keyTokens = PostProcessHelper.Select(arrayItem, keyPart);

                        if (keyTokens.Any())
                        {
                            keyValue += setting.CompositeKeySeparator + GetKeyValue(keyTokens, setting.KeyPartsSeparator);
                        }
                    }

                    var indexValue = objectFromArray[keyValue];
                    var value = PostProcessHelper.CloneJsonNode(arrayItem);

                    if (indexValue == null)
                    {
                        objectFromArray[keyValue] = value;
                    }
                    else if (indexValue is JsonArray)
                    {
                        indexValue.AsArray().Add(value);
                    }
                    else
                    {
                        objectFromArray.Remove(keyValue);
                        objectFromArray[keyValue] = new JsonArray(indexValue, value);
                    }
                }
                product.Add(setting.Name, objectFromArray);
            }
        }

        private static JsonNode NearestAncestorArrayElem(JsonNode node)
        {
            while (node.Parent != null && node.Parent is not JsonArray)
            {
                node = node.Parent;
            }

            return node;
        }

        private static string GetKeyValue(IEnumerable<JsonNode> keyToken, string separator)
        {
            return string.Join(separator, keyToken.Select(x => x.ToString()).OrderBy(x => x));
        }
    }
}
