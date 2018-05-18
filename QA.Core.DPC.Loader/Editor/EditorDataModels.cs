using System;
using System.Collections.Generic;

namespace QA.Core.DPC.Loader.Editor
{
    public class ContentObject : Dictionary<string, object>, ICloneable
    {
        internal const string IdProp = "Id";
        internal const string TimestampProp = "Timestamp";
        internal static string ExtensionProp(string prop) => $"{prop}_Contents";

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

    public class ExtensionFieldObject : Dictionary<string, ContentObject>, ICloneable
    {
        public object Clone()
        {
            var copy = new ExtensionFieldObject();
            foreach (KeyValuePair<string, ContentObject> content in this)
            {
                copy[content.Key] = (ContentObject)content.Value.Clone();
            }
            return copy;
        }
    }
}
