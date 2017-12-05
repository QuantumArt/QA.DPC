using System.Collections.Generic;

namespace QA.Core.DPC.QP.Services
{
    public interface IFactory
    {
        T Resolve<T>(string key);
        void Register(string key);
        void Clear(string key);
        void Clear();
        Dictionary<string, string> Invalidator { get; }
    }
}