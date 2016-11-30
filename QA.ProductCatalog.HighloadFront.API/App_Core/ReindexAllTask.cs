using System;
using QA.ProductCatalog.HighloadFront.Importer;
using QA.ProductCatalog.Infrastructure;

namespace QA.ProductCatalog.HighloadFront.App_Core
{
    public class ReindexAllTask : ITask
    {
        private readonly ProductImporter _importer;
        private readonly ProductManager _manager;
        private readonly IndexOperationSyncer _syncer;

        private const int _lockTimeoutInMs = 5000;

        public ReindexAllTask(ProductImporter importer, ProductManager manager, IndexOperationSyncer syncer)
        {
            _importer = importer;

            _manager = manager;

            _syncer = syncer;
        }


        public void Run(string data, string config, byte[] binData, ITaskExecutionContext executionContext)
        {
            if (_syncer.EnterSyncAll(_lockTimeoutInMs))
            {
                try
                {
                    var itms = data.Split('/');
                    string language = itms[0];
                    string state = itms[1];

                    _manager.DeleteAllASync(language, state).Wait();

                    _importer.ImportAsync(executionContext, language, state).Wait();
                }
                finally
                {
                    _syncer.LeaveSyncAll();
                }
            }
            else
                throw new Exception($"Не удалось войти в EnterSyncAll в течение {_lockTimeoutInMs} миллисекунд");
        }
    }
}