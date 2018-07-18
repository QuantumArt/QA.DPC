using System;
using System.Collections.Generic;
using System.Globalization;
using QA.Core.Models;

namespace QA.Core.DPC.UI.Converters
{
    public class DictionaryConverter<Tin, Tout> : IValueConverter
    {
        private Dictionary<Tin, Tout> _values;

        public Dictionary<Tin, Tout> Values
        {
            get { return _values; }
        }

        public Tout DefaultValue { get; set; }

        public DictionaryConverter(Dictionary<Tin, Tout> values)
        {
            _values = values;
        }

        public DictionaryConverter()
            : this(new Dictionary<Tin, Tout>())
        {

        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Tin))
                return (object)DefaultValue;

            Tout result;
            if (_values.TryGetValue((Tin)value, out result))
            {
                return (object) result;
            }

            return (object) DefaultValue;
        }
    }
}

