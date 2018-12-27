using System;
using System.Threading;
using QA.Core.Cache;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;

namespace QA.Core.DPC.QP.Cache
{
    public class CustomerCacheItemWatcher : CacheItemWatcherBase
    {
        public CustomerCacheItemWatcher(InvalidationMode mode, IContentInvalidator invalidator,
            IConnectionProvider connectionProvider, ILogger logger)
            : this(mode, Timeout.InfiniteTimeSpan, invalidator, connectionProvider, logger, 0, false)
        {

        }

        public CustomerCacheItemWatcher(InvalidationMode mode, TimeSpan pollPeriod, IContentInvalidator invalidator,
            IConnectionProvider connectionProvider, ILogger logger,
            int dueTime = 0,
            bool useTimer = true,
            Func<Tuple<int[], string[]>, bool> onInvalidate = null)
            : base(mode, pollPeriod, invalidator, connectionProvider?.GetConnection(), logger, dueTime,
                useTimer, onInvalidate)
        {

        }
    }
}
