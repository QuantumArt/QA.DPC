using System;
using System.Threading;

namespace QA.ProductCatalog.Infrastructure
{
    public class WarmUpRepeater
    {
        private readonly Timer _timer;

        public WarmUpRepeater(int interval)
        {
            _timer = new Timer(OnTick, null, TimeSpan.Zero, TimeSpan.FromMinutes(interval));
        }

        private static void OnTick(object state)
        {
            WarmUpHelper.WarmUp();
        }

        public void Stop()
        {
            _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }
    }
}