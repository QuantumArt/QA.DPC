using QA.Core.DPC.QP.Services;
using NLog;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.ProductCatalog.ContentProviders;
using Quantumart.QP8.BLL;
using Quantumart.QPublishing.Database;
using System;
using System.Data.SqlClient;

namespace QA.Core.ProductCatalog.Actions.Actions
{
    public class CleanProductVersionAction : ActionTaskBase
    {
        #region Queries
        private const string ProductVersionCleanQuery = @"
            with t as (
            select top({0}) v.[Id] from
	            ProductVersions v with (nolock)
            where
	            exists (
		            select null
		            from ProductVersions as v2 with (nolock)
		            where
			            v2.[Id] > v.[Id]
			            and v2.[DpcId] = v.[DpcId]
			            and v2.[IsLive] = v.[IsLive]
			            and v2.[Language] = v.[Language]
			            and v2.[Format] = v.[Format]
			            and v2.[Modification] < @date)
	            and v.[Modification] < @date
            order by v.[Id])
            delete from t 

            select @@ROWCOUNT";

        private const string ProductVersionCountQuery = @"
            declare @versions int
            declare @products int

            select @versions = count(*)
            from ProductVersions with (nolock)
            where [Modification] < @date

            select @products = count(*)
            from (
	            select [DpcId] from ProductVersions v with (nolock)
	            where [Modification] < @date
	            group by [DpcId], [IsLive], [Language], [Format]
            ) v

            select @versions - @products [Count]";
        #endregion

        #region Constants and fields
        private const int DefaultChunkSize = 100;
        private const int DefaultTimeout = 30;

        private static readonly object Locker = new object();
        private static bool IsProcessing = false;
        
        private readonly ISettingsService _settingsService;
        private readonly string _connectionString;
        #endregion

        public CleanProductVersionAction(ISettingsService settingsService, IConnectionProvider connectionProvider)
        {
            _settingsService = settingsService;
            _connectionString = connectionProvider.GetConnection();
        }

        public override ActionTaskResult Process(ActionContext context)
        {
            bool canProcess = false;
            lock (Locker)
            {
                if (!IsProcessing)
                {
                    IsProcessing = true;
                    canProcess = true;
                }
            }

            if (canProcess)
            {
                try
                {
                    if (!int.TryParse(_settingsService.GetSetting(SettingsTitles.PRODUCTVERSION_CHUNK_SIZE), out int chunkSize))
                    {
                        chunkSize = DefaultChunkSize;
                    }

                    if (!int.TryParse(_settingsService.GetSetting(SettingsTitles.PRODUCTVERSION_TIMEOUT), out int timeout))
                    {
                        timeout = DefaultTimeout;
                    }

                    if (!int.TryParse(_settingsService.GetSetting(SettingsTitles.PRODUCTVERSION_CLEANUP_INTERVAL), out int cleanupInterval))
                    {
                        throw new Exception($"Setting {SettingsTitles.PRODUCTVERSION_CLEANUP_INTERVAL} is not provided or incorrect");
                    }

                    var date = DateTime.Now.AddDays(-cleanupInterval);
                    var expectedTotalCount = GetVersionsCount(date);
                    int processedCount = 0;
                    int currentCount = 0;
                    byte progress = 0;

                    Logger.Info($"Start CleanProductVersionAction cleanupInterval={cleanupInterval}, chunkSize={chunkSize}, expectedTotalCount={expectedTotalCount}");

                    do
                    {
                        currentCount = CleanVersions(date, chunkSize, timeout);
                        processedCount += currentCount;
                        Logger.Info($"Clean {currentCount} product versions");

                        progress = expectedTotalCount == 0 ? (byte)100 : Math.Min((byte)(processedCount * 100 / expectedTotalCount), (byte)100);
                        TaskContext.SetProgress(progress);

                        if (TaskContext.IsCancellationRequested)
                        {
                            TaskContext.IsCancelled = true;
                            break;
                        }
                    }
                    while (currentCount > 0);

                    Logger.Info( $"End CleanProductVersionAction processedCount={processedCount}");

                    return ActionTaskResult.Error($"Cleaned {processedCount} product versions earlier than {date} with chunk size = {chunkSize}");
                }
                finally
                {
                    IsProcessing = false;
                }
            }
            else
            {
                return ActionTaskResult.Error("CleanProductVersionAction is already running");
            }
        }

        #region Private methods
        private int GetVersionsCount(DateTime date)
        {
            var connector = GetConnector();
            var sqlCommand = new SqlCommand(ProductVersionCountQuery);
            sqlCommand.Parameters.AddWithValue("@date", date);
            var data = connector.GetRealScalarData(sqlCommand);
            return (int)data;
        }

        private int CleanVersions(DateTime date, int chunkSize, int timeout)
        {
            var connector = GetConnector();
            var query = string.Format(ProductVersionCleanQuery, chunkSize);
            var sqlCommand = new SqlCommand(query);
            sqlCommand.CommandTimeout = timeout;
            sqlCommand.Parameters.AddWithValue("@date", date);
            var data = connector.GetRealScalarData(sqlCommand);
            return (int)data;
        }

        private DBConnector GetConnector()
        {
            var scope = QPConnectionScope.Current;

            if (scope != null && scope.DbConnection != null)
            {
                return new DBConnector(scope.DbConnection);
            }
            else
            {
                return new DBConnector(_connectionString);
            }
        }
        #endregion
    }
}
