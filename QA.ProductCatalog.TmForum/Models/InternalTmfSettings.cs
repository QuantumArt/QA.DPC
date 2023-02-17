using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace QA.ProductCatalog.TmForum.Models
{
    internal static class InternalTmfSettings
    {
        internal const string FieldsQueryParameterName = "fields";
        internal const string OffsetQueryParameterName = "offset";
        internal const string LimitQueryParameterName = "limit";
        internal const string LastUpdateParameterName = "lastUpdate";
        internal const string ExternalIdFieldName = "id";
        internal const string EntitySeparator = ".";
        internal const string VersionFieldName = "version";
        internal const string ArticleStateErrorText = "Update for product which part is splitted or not published is restricted.";
        internal const string ApiPrefix = "api";
        internal const string TmfItemIdentifier = "tmf";
        internal const string ArticleFilterContextItemName = "ArticleFilter";
        internal const string RegionTagsContextItemName = "includeRegionTags";

        internal static readonly string[] DefaultTmfFieldsToSelect = new string[1] { "id" };
        internal static readonly PathString ApiPathPrefix = "/" + ApiPrefix.TrimStart('/');
        internal static readonly string[] ArticleStateErrorArray = new string[1] { ArticleStateErrorText };
        internal static readonly Regex IdRegex = new("(.*)\\([Vv]ersion=(.*)\\)", RegexOptions.Compiled);
        internal static readonly IReadOnlyCollection<string> ReservedSearchParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            FieldsQueryParameterName,
            OffsetQueryParameterName,
            LimitQueryParameterName,
            LastUpdateParameterName
        };
        internal static readonly IReadOnlyCollection<string> NotUpdatableFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "id",
            "href",
            "lastUpdate",
            "@type",
            "@baseType"
        };
    }
}
