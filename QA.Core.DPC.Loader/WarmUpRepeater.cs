using System;
using System.Threading;
using Microsoft.Extensions.Options;
using QA.Core.DPC.QP.Services;

namespace QA.Core.DPC.Loader
{
    public class WarmUpRepeater
    {
        private readonly LoaderProperties _props;
        private Timer _timer;
        private IWarmUpProvider _provider;

        public WarmUpRepeater(IOptions<LoaderProperties> props, IWarmUpProvider provider)
        {
            _provider = provider;
            _props = props.Value;
        }
        public void Start()
        {
            if (_props.LoaderWarmUpProductId != 0)
            {
                _provider.WarmUp();
                if (_props.LoaderWarmUpRepeatInMinutes != 0)
                {
                    _timer = new Timer(OnTick, null, TimeSpan.Zero, TimeSpan.FromMinutes(_props.LoaderWarmUpRepeatInMinutes));               
                }
            }
        }

        private void OnTick(object state)
        {
            _provider.WarmUp();
        }

        public void Stop()
        {
            if (_props.LoaderWarmUpRepeatInMinutes > 0)
            {
                _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            }
        }
    }
}