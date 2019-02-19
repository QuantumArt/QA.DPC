using Quantumart.QP8.BLL;
using System;
using System.Configuration;
using System.Transactions;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;

namespace QA.Core.ProductCatalog.Actions.Services
{
    public sealed class Transaction : ITransaction
    {
        private readonly TransactionScope _transactionScope;
        private readonly QPConnectionScope _connectionScope;

        public Transaction(IConnectionProvider connectionProvider, ILogger logger)
        {
            _transactionScope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { Timeout = connectionProvider.TransactionTimeout, IsolationLevel = IsolationLevel.ReadUncommitted });
            var current = QPConnectionScope.Current;

            if (current != null && current.DbConnection != null)
            {
                logger.Error("Попытка создать транзакцию на существующем подключении к БД. Статус подключения: " + current.DbConnection.State);
            }

            _connectionScope = new QPConnectionScope(connectionProvider.GetConnection());
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