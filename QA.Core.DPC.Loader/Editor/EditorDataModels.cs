using System.Collections.Generic;

namespace QA.Core.DPC.Loader.Editor
{
    public class ArticleObject : Dictionary<string, object>
    {
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
