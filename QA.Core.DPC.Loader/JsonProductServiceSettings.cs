﻿using Newtonsoft.Json;

namespace QA.Core.DPC.Loader
{
    public class JsonProductServiceSettings
    {
        private const string DefaultWrapperName = "product";
        private static readonly JsonSerializerSettings _defaultSerializerSettings
            = new() { Formatting = Formatting.Indented };

        public string WrapperName { get; init; } = DefaultWrapperName;

        public JsonSerializerSettings SerializerSettings { get; init; } = _defaultSerializerSettings;

        public bool IsWrapped => !string.IsNullOrWhiteSpace(WrapperName);
    }
}
