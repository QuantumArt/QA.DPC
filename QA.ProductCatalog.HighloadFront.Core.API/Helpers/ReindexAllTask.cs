﻿using System;
using System.Collections.Generic;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.HighloadFront.Interfaces;
using System.Linq;

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
                        var indexesToDelete = _manager.GetIndexesAsync(language, state, _stores).Result;
                        var newIndex = _manager.CreateVersionedIndexAsync(language, state, _stores).Result;

                        if (string.IsNullOrWhiteSpace(newIndex))
                        {
                            executionContext.Result = ActionTaskResult.Error("Unable to create new index.");
                            return;
                        }

                        _importer.ImportAsync(executionContext, language, state, _stores, newIndex).Wait();

                        string alias = _configuration.GetElasticIndex(language, state).Name;

                        if (indexesToDelete.Contains(alias))
                        {
                            DeleteIndexes();
                            _manager.AddIndexToAliasAsync(language, state, _stores, newIndex, alias).Wait();
                        }
                        else
                        {
                            _manager.AddIndexToAliasAsync(language, state, _stores, newIndex, alias).Wait();
                            DeleteIndexes();
                        }

                        void DeleteIndexes()
                        {
                            foreach(string index in indexesToDelete)
                            {
                                _manager.DeleteIndexByNameAsync(language, state, _stores, index).Wait();
                            }
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