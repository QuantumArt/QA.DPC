using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration.Json;

namespace QA.ProductCatalog.HighloadFront.Core.API.Configurations
{
    public class JsonEnvironmentConfigurationProvider : JsonConfigurationProvider
    {
        private static readonly Regex EnvVarPattern = new(@"\${(.+?)}", RegexOptions.Compiled);

        public JsonEnvironmentConfigurationProvider(JsonConfigurationSource source) : base(source)
        {
        }

        public override void Load(Stream stream)
        {
            base.Load(stream);

            var envVariables = Environment.GetEnvironmentVariables()
                .Cast<DictionaryEntry>()
                .ToDictionary(d => d.Key as string, d => d.Value as string, StringComparer.OrdinalIgnoreCase);

            foreach (var appSetting in Data)
            {
                if (string.IsNullOrWhiteSpace(appSetting.Value))
                {
                    continue;
                }

                var matches = EnvVarPattern.Matches(appSetting.Value);
                if (!matches.Any())
                {
                    continue;
                }

                var template = new StringBuilder(appSetting.Value);

                foreach (var match in matches.ToArray())
                {
                    var templatePartToReplace = match.Value;
                    var templatePartKey = match.Groups[1].Value;

                    if (!envVariables.TryGetValue(templatePartKey, out var envVarValue))
                    {
                        throw new Exception($"Переменная Environment {templatePartKey} не задана");
                    }

                    template.Replace(templatePartToReplace, envVarValue);
                }

                Data[appSetting.Key] = template.ToString();
            }
        }
    }
}