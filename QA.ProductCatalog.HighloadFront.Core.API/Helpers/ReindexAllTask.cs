using System;
using System.Collections.Generic;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.HighloadFront.Interfaces;

namespace QA.ProductCatalog.HighloadFront.Core.API.Helpers
{
    public class ReindexAllTask : ITask
    {
        private readonly ProductImporter _importer;
        private readonly ProductManager _manager;
        private readonly ElasticConfiguration _configuration;
        private readonly Dictionary<string, IProductStore> _stores;
        private const int LockTimeoutInMs = 5000;

        
        public ReindexAllTask(ProductImporter importer, ProductManager manager, ElasticConfiguration configuration, Dictionary<string, IProductStore> stores)
        {
            _importer = importer;
            _manager = manager;
            _configuration = configuration;
            _stores = stores;
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
                        string alias = _configuration.GetElasticIndex(language, state).Name;
                        var indexesToDelete = _manager.GetIndexesToDeleteAsync(language, state, _stores, alias).Result;
                        var newIndex = _manager.CreateVersionedIndexAsync(language, state, _stores).Result;

                        if (string.IsNullOrWhiteSpace(newIndex))
                        {
                            executionContext.Result = ActionTaskResult.Error("Unable to create new index.");
                            return;
                        }

                        _importer.ImportAsync(executionContext, language, state, _stores, newIndex).Wait();


                        if (indexesToDelete.Contains(alias))
                        {
                            _manager.DeleteIndexesByNamesAsync(language, state, _stores, indexesToDelete).Wait();
                            _manager.ReplaceIndexesInAliasAsync(language, state, _stores, newIndex, alias, Array.Empty<string>()).Wait();
                        }
                        else
                        {
                            var indexesInAlias = _manager.GetIndexesInAliasAsync(language, state, _stores).Result;
                            _manager.ReplaceIndexesInAliasAsync(language, state, _stores, newIndex, alias, indexesInAlias).Wait();
                            _manager.DeleteIndexesByNamesAsync(language, state, _stores, indexesToDelete).Wait();
                        }
                    }
                    else
                    {
                        throw new Exception($"Unable to validate InstanceId");
                    }
                }
                finally
                {
                    syncer.LeaveSyncAll();
                }
            }
            else
                throw new Exception($"Unable to enter into EnterSyncAll during {LockTimeoutInMs} ms");
        }
    }
}