using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Infrastructure;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class ArrayIndexer : IProductPostProcessor
    {
        private readonly ArrayIndexingSettings[] _arrayIndexingSettings;

        public ArrayIndexer(ArrayIndexingSettings[] arrayIndexingSettings)
        {
            _arrayIndexingSettings = arrayIndexingSettings;
        }

        public JObject Process(ProductPostProcessorData data)
        {
            var product = data.Product;

            foreach (var setting in _arrayIndexingSettings)
            {
                JToken array = product.SelectToken(setting.Path);

                if (array == null)
                    continue;

                if (array.Type != JTokenType.Array)
                    throw new Exception($"Тип элемента по пути {setting.Path} должен быть массивом");

                var topKeyTokens = array.SelectTokens("[*]." + setting.Keys[0]);

                if (!topKeyTokens.Any())
                    continue;

                JObject objectFromArray = new JObject();

                foreach (var topKeyTokensWithArrayItem in topKeyTokens.GroupBy(x => x.Ancestors().First(y => y.Parent == array)))
                {
                    JToken arrayItem = topKeyTokensWithArrayItem.Key;

                    string keyValue = GetKeyValue(topKeyTokensWithArrayItem, setting.KeyPartsSeparator);

                    if (string.IsNullOrEmpty(keyValue))
                        continue;

                    RemoveProperties(topKeyTokensWithArrayItem);

                    foreach (string keyPart in setting.Keys.Skip(1))
                    {
                        var keyTokens = arrayItem.SelectTokens(keyPart);

                        if (keyTokens.Any())
                        {
                            keyValue += setting.CompositeKeySeparator + GetKeyValue(keyTokens, setting.KeyPartsSeparator);

                            RemoveProperties(keyTokens);
                        }
                    }

                    var indexValue = objectFromArray[keyValue];

                    if (indexValue == null)
                    {
                        indexValue = objectFromArray[keyValue] = arrayItem.DeepClone();
                    }
                    else if (indexValue.Type == JTokenType.Array)
                    {
                        ((JArray)indexValue).Add(arrayItem);
                    }
                    else
                    {
                        objectFromArray[keyValue] = new JArray(indexValue, arrayItem);
                    }
                }

                array.Parent.Parent.Add(new JProperty(setting.Name, objectFromArray));
            }

            return product;
        }

        private static void RemoveProperties(IEnumerable<JToken> values)
        {
            foreach (var value in values)
            {
                value.Parent.Remove();
            }
        }

        private static string GetKeyValue(IEnumerable<JToken> keyToken, string separator)
        {
            return string.Join(separator, keyToken.Select(x => x.ToString()).OrderBy(x => x));
        }
    }
}
