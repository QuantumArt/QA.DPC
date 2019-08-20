using QA.Core.Cache;
using QA.Core.DPC.Loader.Services;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
#if !NETSTANDARD
using System.Xaml;
#else
using Portable.Xaml;
#endif

using System.IO;
using System.IO.Compression;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.ProductCatalog.ContentProviders;

namespace QA.Core.DPC.Loader
{
    public class LocalSystemCachedLoader : ProductLoader
    {
        /// <summary>
        /// Архивация файлов с  продуктами.
        /// По умолчанию True
        /// </summary>
        public bool ArchiveFiles { get; set; }

        public LocalSystemCachedLoader(IContentDefinitionService definitionService,
            ILogger logger,
            VersionedCacheProviderBase cacheProvider,
            ICacheItemWatcher cacheItemWatcher,
            IReadOnlyArticleService articleService,
            IFieldService fieldService,
            ISettingsService settingsService,
            IList<IConsumerMonitoringService> consumerMonitoringServices,
            IArticleFormatter formatter, IConnectionProvider connectionProvider) : base(definitionService,
                logger, cacheProvider, cacheItemWatcher, articleService, fieldService, settingsService, consumerMonitoringServices, formatter, connectionProvider)
        {
            DataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
            if (DataDirectory == null)
            {
                throw new InvalidOperationException("Должен быть определен DataDirectory");
            }
            ArchiveFiles = true;
        }

        public string DataDirectory { get; set; }

        public override Article GetProductById(int id, bool isLive = false, ProductDefinition productDefinition = null)
        {
            return GetOrAdd(string.Format("GetProductById_{0}_{1}_{2}.xaml", id, isLive, productDefinition?.GetHashCode()),
                () => base.GetProductById(id, isLive, productDefinition));
        }

        public override ProductDefinition GetProductDefinition(int productTypeId, bool isLive = false)
        {
            return base.GetProductDefinition(productTypeId, isLive);
        }

        public override ProductDefinition GetProductDefinition(int productTypeId, int contentId, bool isLive = false)
        {
            return base.GetProductDefinition(productTypeId, contentId, isLive);
        }

        protected virtual T GetOrAdd<T>(string key, Func<T> func)
        {
            var paths = new List<string>() { Path.Combine(DataDirectory, key) };
            if (ArchiveFiles)
            {
                paths.Insert(0, Path.Combine(DataDirectory, $"{key}.zip"));
            }
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    if (path.EndsWith("zip"))
                    {
                        using (var archive = ZipFile.OpenRead(path))
                        {
                            var entry = archive.Entries.FirstOrDefault();
                            using (var stream = entry.Open())
                            {
                                return (T)XamlServices.Load(stream);
                            }
                        }
                    }
                    else
                    {
                        return (T)XamlServices.Load(path);
                    }

                }
            }

            var data = func();
            if (data != null)
            {
                var path = paths[0];
                if (path.EndsWith("zip"))
                {
                    using (var memoryStream = File.Create(path))
                    {
                        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create))
                        {
                            var entry = archive.CreateEntry(key);

                            using (var stream = entry.Open())
                            {
                                XamlServices.Save(stream, data);
                            }
                        }
                    }
                }
            }

            return data;
        }

    }
}
