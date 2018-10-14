using System;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.ContentProviders;

namespace QA.ProductCatalog.HighloadFront.Core.API.Helpers
{
    public class ReindexAllTask : ITask
    {
        private readonly ProductImporter _importer;
        private readonly ProductManager _manager;
        private readonly IElasticConfiguration _configuration;

        private const int LockTimeoutInMs = 5000;

        public ReindexAllTask(ProductImporter importer, ProductManager manager, IElasticConfiguration configuration)
        {
            _importer = importer;
            _manager = manager;
            _configuration = configuration;
        }

        public void Run(string data, string config, byte[] binData, ITaskExecutionContext executionContext)
        {
            var itms = data.Split('/');
            string language = itms[0];
            string state = itms[1];

            var syncer = _configuration.GetSyncer(language, state);

            if (syncer.EnterSyncAll(LockTimeoutInMs))
            {
                try
                {
                    if (_importer.ValidateInstance(language, state))
                    {
                        _manager.DeleteAllASync(language, state).Wait();
                        _importer.ImportAsync(executionContext, language, state).Wait();
                    }
                    else
                    {
                        throw new Exception($"Не удалось пройти валидацию по InstanceId");
                    }
                }
                finally
                {
                    syncer.LeaveSyncAll();
                }
            }
            else
                throw new Exception($"Не удалось войти в EnterSyncAll в течение {LockTimeoutInMs} миллисекунд");
        }
    }
}