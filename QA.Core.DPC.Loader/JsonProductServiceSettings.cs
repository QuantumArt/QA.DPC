using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace QA.Core.DPC.Loader
{
    public class JsonProductServiceSettings
    {
        private const string DefaultWrapperName = "product";
        private static readonly JsonSerializerSettings _defaultSerializerSettings
            = new() { Formatting = Formatting.Indented };

        public string WrapperName { get; set; } = DefaultWrapperName;

        public JsonSerializerSettings SerializerSettings { get; init; } = _defaultSerializerSettings;

        public bool IsWrapped => !string.IsNullOrWhiteSpace(WrapperName);

        public ICollection<string> Fields { get; init; } = Array.Empty<string>();
    }
}
