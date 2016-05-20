using QA.Core.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.DPC.Loader
{
    public class DPCContentInvalidator : ContentInvalidator 
    {
        public static string[] GetTagNameByContentId(params int[] contentIds)
        {
			return GetTagNameByContentId((IEnumerable<int>)contentIds);
        }

		public static string[] GetTagNameByContentId(IEnumerable<int> contentIds)
		{
			return contentIds.Select(x => x.ToString()).Distinct().ToArray();
		}

        public DPCContentInvalidator(IVersionedCacheProvider cacheProvider) : base (cacheProvider)
        {

        }
        protected override string[] ResolveKeys(int[] contentIds)
        {
            return GetTagNameByContentId(contentIds);
        }

        protected override string[] ResolveTableNames(string[] tableNames)
        {
            return tableNames;
        }
    }
}
