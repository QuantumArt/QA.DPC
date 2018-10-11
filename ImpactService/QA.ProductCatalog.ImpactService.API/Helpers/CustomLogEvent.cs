using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging.Internal;

namespace QA.ProductCatalog.ImpactService.API.Helpers
{
    class CustomLogEvent : IReadOnlyList<KeyValuePair<string, object>>
    {
        readonly string _format;
        readonly object[] _parameters;

        FormattedLogValues _logValues;
        FormattedLogValues LogValues => _logValues ?? (_logValues = new FormattedLogValues(_format, _parameters));

        List<KeyValuePair<string, object>> _extraProperties;

        public CustomLogEvent(string format, params object[] values)
        {
            _format = format;
            _parameters = values;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            if (MessagePropertyCount == 0)
            {
                if (ExtraPropertyCount > 0)
                    return _extraProperties.GetEnumerator();
                else
                    return Enumerable.Empty<KeyValuePair<string, object>>().GetEnumerator();
            }
            else
            {
                if (ExtraPropertyCount > 0)
                    return _extraProperties.Concat(LogValues).GetEnumerator();
                else
                    return LogValues.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private int MessagePropertyCount
        {
            get
            {
                if (LogValues.Count > 1 && !string.IsNullOrEmpty(LogValues[0].Key) &&
                    !char.IsDigit(LogValues[0].Key[0]))
                    return LogValues.Count;
                else
                    return 0;
            }
        }

        private int ExtraPropertyCount => _extraProperties?.Count ?? 0;

        public int Count => MessagePropertyCount + ExtraPropertyCount;

        public KeyValuePair<string, object> this[int index]
        {
            get
            {
                int extraCount = ExtraPropertyCount;
                if (index < extraCount)
                {
                    return _extraProperties[index];
                }
                else
                {
                    return LogValues[index - extraCount];
                }
            }
        }

        public CustomLogEvent AddProp(string name, object value)
        {
            var properties = _extraProperties ?? (_extraProperties = new List<KeyValuePair<string, object>>());
            properties.Add(new KeyValuePair<string, object>(name, value));
            return this;
        }

        public static Func<CustomLogEvent, Exception, string> Formatter { get; } = (l, e) => l.LogValues.ToString();
    };
}