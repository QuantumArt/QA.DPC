using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QA.ProductCatalog.HighloadFront.App_Core
{
    public class IndexOperationSyncer
    {
        private SemaphoreSlim _semaphore;
        private readonly ushort _maxConcurrentSingleCRUD;

        public IndexOperationSyncer(ushort maxConcurrentSingleCRUD = 10)
        {
            _semaphore = new SemaphoreSlim(maxConcurrentSingleCRUD, maxConcurrentSingleCRUD);

            _maxConcurrentSingleCRUD = maxConcurrentSingleCRUD;
        }


        public bool AnySlotsLeft { get { return _semaphore.CurrentCount > 0; } }

        public Task<bool> EnterSingleCRUDAsync(int millisecondsTimeout)
        {
            return _semaphore.WaitAsync(millisecondsTimeout);
        }

        public void LeaveSingleCRUD()
        {
            _semaphore.Release();
        }


        public bool EnterSyncAll(int millisecondsTimeout)
        {
            var enterTasks = new List<Task<bool>>(_maxConcurrentSingleCRUD);

            for (ushort i = 0; i < _maxConcurrentSingleCRUD; i++)
            {
                enterTasks.Add(_semaphore.WaitAsync(millisecondsTimeout));
            }

            Task.WaitAll(enterTasks.ToArray());

            if (enterTasks.All(x => x.Result))
                return true;
            else
            {
                _semaphore.Release(enterTasks.Count(x => !x.Result));

                return false;
            }
        }


        public void LeaveSyncAll()
        {
            _semaphore.Release(_maxConcurrentSingleCRUD);
        }
    }

}