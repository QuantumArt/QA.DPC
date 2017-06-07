using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Models;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.PostProcessing
{
    public class ArrayIndexer : IProductPostProcessor
    {
        private readonly ArrayIndexingSettings[] _arrayIndexingSettings;

        public ArrayIndexer(IOptions<SonicElasticStoreOptions> elasticOptions)
        {
            _arrayIndexingSettings = elasticOptions?.Value?.ArrayIndexingSettings;
        }

        public JObject Process(ProductPostProcessorData data)
        {
            var product = data.Product;

            if (_arrayIndexingSettings == null)
                return product;

            foreach (var setting in _arrayIndexingSettings)
            {
                JToken array = product.SelectToken(setting.Path);

                if (array == null)
                    continue;

                if (array.Type != JTokenType.Array)
                    throw new Exception($"Тип элемента по пути {setting.Path} должен быть массивом");

                var topKeyTokens = array.SelectTokens("[*]." + setting.Keys[0]).ToArray();

                if (!topKeyTokens.Any())
                    continue;

                JObject objectFromArray = new JObject();

                foreach (var topKeyTokensWithArrayItem in topKeyTokens.GroupBy(x => x.Ancestors().First(y => y.Parent == array)))
                {
                    JToken arrayItem = topKeyTokensWithArrayItem.Key;

                    string keyValue = GetKeyValue(topKeyTokensWithArrayItem, setting.KeyPartsSeparator);

                    if (string.IsNullOrEmpty(keyValue))
                        continue;

                    foreach (string keyPart in setting.Keys.Skip(1))
                    {
                        var keyTokens = arrayItem.SelectTokens(keyPart).ToArray();

                        if (keyTokens.Any())
                        {
                            keyValue += setting.CompositeKeySeparator + GetKeyValue(keyTokens, setting.KeyPartsSeparator);
                        }
                    }

                    var indexValue = objectFromArray[keyValue];

                    if (indexValue == null)
                    {
                        objectFromArray[keyValue] = arrayItem.DeepClone();
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

        private static string GetKeyValue(IEnumerable<JToken> keyToken, string separator)
        {
            return string.Join(separator, keyToken.Select(x => x.ToString()).OrderBy(x => x));
        }
    }
}
