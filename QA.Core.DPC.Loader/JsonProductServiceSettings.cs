using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;

namespace QA.Core.DPC.Loader
{
    public class JsonProductServiceSettings
    {
        private const string DefaultWrapperName = "product";
        private static readonly JsonSerializerSettings _defaultSerializerSettings
            = new() { Formatting = Formatting.Indented };
        
        private static readonly JsonSerializerSettings _isoSerializerSettings
            = new() { Formatting = Formatting.Indented, Converters = { new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss.fffzzz" } } };

        public string WrapperName { get; set; } = DefaultWrapperName;

        public JsonSerializerSettings SerializerSettings { get; init; } = _defaultSerializerSettings;
        
        public JsonSerializerSettings IsoSerializerSettings { get; init; } = _isoSerializerSettings;

        public bool IsWrapped => !string.IsNullOrWhiteSpace(WrapperName);

        public ICollection<string> Fields { get; init; } = Array.Empty<string>();
    }
}
