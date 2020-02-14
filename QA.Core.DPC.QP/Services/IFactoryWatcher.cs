using QA.Core.DPC.QP.Models;
using System;

namespace QA.Core.DPC.QP.Services
{
    public interface IFactoryWatcher
    {
        void Start();
        void Stop();
        void Watch();
        event EventHandler<FactoryWatcherEventArgs> OnConfigurationModify;
    }
}
