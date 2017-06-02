using System.Collections.Generic;
using QA.DPC.Core.Helpers;

namespace QA.DPC.Core.Helpers
{

    public interface IVersionedCacheCoreProviderCollection
    {
        VersionedCacheCoreProvider Get(string customerCode);
    }

    public class VersionedCacheCoreProviderCollection : IVersionedCacheCoreProviderCollection
    {
        private static readonly object Locker = new object();

        private readonly Dictionary<string, VersionedCacheCoreProvider> _list = new Dictionary<string, VersionedCacheCoreProvider>();

        public VersionedCacheCoreProvider Get(string customerCode)
        {
            VersionedCacheCoreProvider result;
            if (!_list.TryGetValue(customerCode, out result))
            {
                lock (Locker)
                {
                    result = new VersionedCacheCoreProvider();
                    _list.Add(customerCode, result);
                }

            }
            return result;
        }
    }
}
