using System;
using System.ComponentModel;

namespace QA.Core.Models.Entities
{
    [Serializable]
    public class PlainArticleField : ArticleField, IGetFieldStringValue
    {
        [DefaultValue(null)]
        public string Value { get; set; }

        [DefaultValue(null)]
        public object NativeValue { get; set; }

        public PlainFieldType PlainFieldType { get; set; }

        public override string ToString()
        {
            return Value;
        }
    }
}