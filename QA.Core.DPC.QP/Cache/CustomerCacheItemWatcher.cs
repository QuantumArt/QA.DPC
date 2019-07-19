using System;
using System.Threading;
using QA.Core.Cache;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using Quantumart.QP8.Constants;

namespace QA.Core.DPC.QP.Cache
{
    public class CustomerCacheItemWatcher : CacheItemWatcherBase
    {
        public CustomerCacheItemWatcher(InvalidationMode mode, IContentInvalidator invalidator,
            IConnectionProvider connectionProvider, ILogger logger, DatabaseType databseType = DatabaseType.SqlServer)
            : this(mode, Timeout.InfiniteTimeSpan, invalidator, connectionProvider, logger, 0, false, databaseType: databseType)
        {

        }

        public CustomerCacheItemWatcher(InvalidationMode mode, TimeSpan pollPeriod, IContentInvalidator invalidator,
            IConnectionProvider connectionProvider, ILogger logger,
            int dueTime = 0,
            bool useTimer = true,
            Func<Tuple<int[], string[]>, bool> onInvalidate = null,
            DatabaseType databaseType = DatabaseType.SqlServer)
            : base(mode, pollPeriod, invalidator, connectionProvider?.GetConnection(), logger, dueTime,
                useTimer, onInvalidate, databaseType.ToString())
        {

        }
    }
}
