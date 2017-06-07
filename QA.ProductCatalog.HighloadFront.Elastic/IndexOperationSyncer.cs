using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class IndexOperationSyncer
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly ushort _maxConcurrentSingleCrud;

        public IndexOperationSyncer(ushort maxConcurrentSingleCrud = 10)
        {
            _semaphore = new SemaphoreSlim(maxConcurrentSingleCrud, maxConcurrentSingleCrud);

            _maxConcurrentSingleCrud = maxConcurrentSingleCrud;
        }


        public bool AnySlotsLeft => _semaphore.CurrentCount > 0;

        public Task<bool> EnterSingleCrudAsync(int millisecondsTimeout)
        {
            return _semaphore.WaitAsync(millisecondsTimeout);
        }

        public void LeaveSingleCrud()
        {
            _semaphore.Release();
        }


        public bool EnterSyncAll(int millisecondsTimeout)
        {
            var enterTasks = new List<Task<bool>>(_maxConcurrentSingleCrud);

            for (ushort i = 0; i < _maxConcurrentSingleCrud; i++)
            {
                enterTasks.Add(_semaphore.WaitAsync(millisecondsTimeout));
            }

            Task.WaitAll(enterTasks.ToArray());

            if (enterTasks.All(x => x.Result))
                return true;
            _semaphore.Release(enterTasks.Count(x => !x.Result));

            return false;
        }


        public void LeaveSyncAll()
        {
            _semaphore.Release(_maxConcurrentSingleCrud);
        }
    }

}