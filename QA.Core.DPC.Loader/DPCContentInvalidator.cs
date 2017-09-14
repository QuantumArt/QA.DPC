using System.Collections.Generic;
using System.Linq;
using QA.Core.Logger;

namespace QA.Core.DPC.Loader
{
    public class DpcContentInvalidator : IContentInvalidator 
    {
        private readonly IVersionedCacheProvider _cacheProvider;
        private readonly ILogger _logger;

        public DpcContentInvalidator(IVersionedCacheProvider cacheProvider, ILogger logger)
        {
            _cacheProvider = cacheProvider;
            _logger = logger;
        }

        public virtual void InvalidateIds(InvalidationMode mode, params int[] contentIds)
        {
            var names = ResolveKeys(contentIds);
            InvalidateKeys(mode, names);
        }

        public virtual void InvalidateKeys(InvalidationMode mode, params string[] keys)
        {
            _logger.Debug(_ => "Invalidating a set of keys " + string.Join(", ", keys));

            if (keys == null || keys.Length == 0)
                return;

            _cacheProvider.InvalidateByTags(mode, keys);
        }

        public virtual void InvalidateTables(InvalidationMode mode, params string[] tableNames)
        {
            var keys = ResolveTableNames(tableNames);
            InvalidateKeys(mode, keys);
        }

        public static string[] GetTagNameByContentId(params int[] contentIds)
        {
			return GetTagNameByContentId((IEnumerable<int>)contentIds);
        }

		public static string[] GetTagNameByContentId(IEnumerable<int> contentIds)
		{
			return contentIds.Select(x => x.ToString()).Distinct().ToArray();
		}

        protected string[] ResolveKeys(int[] contentIds)
        {
            return GetTagNameByContentId(contentIds);
        }

        protected string[] ResolveTableNames(string[] tableNames)
        {
            return tableNames;
        }
    }
}
