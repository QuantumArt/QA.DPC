using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.DPC.Loader
{
    public static class CacheTags
    {
        public static class QP8
        {
            public const string StatusType = "STATUS_TYPE";
            public const string User = "USERS";
            public const string Field = "CONTENT_ATTRIBUTE";
            public const string Content = "CONTENT";
            public const string Site = "SITE";
            public const string DB = "DB";
            public const string AppSettings = "APP_SETTINGS";

            public static string[] All = new string[] { StatusType, User, Field, Content, Site, DB, AppSettings };
        }

        public static string[] Merge(params string[] tags)
        {
            return tags;
        }
    }
}
