using System;

namespace QA.ProductCatalog.HighloadFront.Core.API.Helpers
{
    public static class DateTimeExtensions
    {
        public static long ToUnixTimestamp(this DateTime date)
        {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
            var unixTimeSpan = date - unixEpoch;

            return (long)unixTimeSpan.TotalSeconds;
        }
    }
}