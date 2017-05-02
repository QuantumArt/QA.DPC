using QA.Core.DPC.QP.Servives;
using Quantumart.QP8.BLL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace QA.Core.ProductCatalog.Actions.Services
{
    public sealed class Transaction : ITransaction
    {
        private readonly TransactionScope _transactionScope;
        private readonly QPConnectionScope _connectionScope;
        private readonly IConnectionProvider _connectionProvider;

        public Transaction(IConnectionProvider connectionProvider, ILogger logger)
        {
            TimeSpan timeout;
            var connectionString = connectionProvider.GetConnection();
            string configTimeout = ConfigurationManager.AppSettings["ProductCatalog.Actions.TransactionTimeout"];

            if (!TimeSpan.TryParse(configTimeout, out timeout))
            {
                timeout = TimeSpan.FromMinutes(3);
            }

            _transactionScope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { Timeout = timeout, IsolationLevel = IsolationLevel.ReadUncommitted });
            var current = QPConnectionScope.Current;

            if (current != null && current.DbConnection != null)
            {
                logger.Error("Попытка создать транзакцию на существующем подключении к БД. Статус подключения: " + current.DbConnection.State);
            }

            _connectionScope = new QPConnectionScope(connectionString);
        }

        #region ITransaction implementation
        public void Commit()
        {
            _transactionScope.Complete();
        }

        public void Dispose()
        {
            _connectionScope.Dispose();
            _transactionScope.Dispose();
        }
        #endregion
    }
}