using System;
using System.Collections.Generic;

namespace QA.Core.DPC.Loader.Editor
{
    public class ContentObject : Dictionary<string, object>, ICloneable
    {
        internal const string IdProp = "Id";
        internal const string ContentIdProp = "ContentId";
        internal const string TimestampProp = "Timestamp";

        public object Clone()
        {
            var copy = new ContentObject();
            foreach (KeyValuePair<string, object> field in this)
            {
                if (field.Value is ICloneable cloneable)
                {
                    copy[field.Key] = cloneable.Clone();
                }
                else
                {
                    copy[field.Key] = field.Value;
                }
            }
            return copy;
        }
    }

    public class FileFieldObject : ICloneable
    {
        public string Name { get; set; }
        public string AbsoluteUrl { get; set; }

        public object Clone()
        {
            return new FileFieldObject
            {
                Name = Name,
                AbsoluteUrl = AbsoluteUrl,
            };
        }
    }

    public class ExtensionFieldObject : ICloneable
    {
        public string Value { get; set; }
        public Dictionary<string, ContentObject> Contents { get; set; }

        public object Clone()
        {
            var copy = new ExtensionFieldObject
            {
                Value = Value,
                Contents = new Dictionary<string, ContentObject>()
            };
            foreach (KeyValuePair<string, ContentObject> content in Contents)
            {
                copy.Contents[content.Key] = (ContentObject)content.Value.Clone();
            }
            return copy;
        }
    }
}
