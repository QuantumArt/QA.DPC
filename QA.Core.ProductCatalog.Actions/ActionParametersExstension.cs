using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.ProductCatalog.Actions
{
	public static class ActionParametersExstension
	{
		private const string ChannelsKey = "Channels";
        private const string ClearFieldIdsKey = "ClearFieldIds";
        public static string[] GetChannels(this Dictionary<string, string> actionParameters)
		{
			string channels;

			if (actionParameters.TryGetValue(ChannelsKey, out channels))
			{
				return channels == null ? null : channels.Split(new[] { ',' });
            }
			else
			{
				return null;
			}
		}

        public static int[] GetClearFieldIds(this Dictionary<string, string> actionParameters)
        {
            string ids;

            if (actionParameters.TryGetValue(ClearFieldIdsKey, out ids))
            {
                return string.IsNullOrWhiteSpace(ids) ? null : ids.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s)).ToArray();
            }
            else
            {
                return null;
            }
        }
    }
}
