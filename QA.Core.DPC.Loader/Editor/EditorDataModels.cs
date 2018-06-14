using System.Collections.Generic;

namespace QA.Core.DPC.Loader.Editor
{
    public class ArticleObject : Dictionary<string, object>
    {
        internal const string Id = "Id";
        internal const string ContentName = "ContentName";
        internal const string Modified = "Modified";
        internal static string Contents(string prop) => $"{prop}_Contents";
    }

    public class FileFieldObject
    {
        public string Name { get; set; }
        public string AbsoluteUrl { get; set; }
    }

    public class ExtensionFieldObject : Dictionary<string, ArticleObject>
    {
    }
}
