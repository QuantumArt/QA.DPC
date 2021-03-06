﻿namespace QA.Core.DPC.QP.Cache
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

            public static string[] All = new string[] { StatusType, User, Field, Content, Site, DB };
        }

        public static string[] Merge(params string[] tags)
        {
            return tags;
        }
    }
}
