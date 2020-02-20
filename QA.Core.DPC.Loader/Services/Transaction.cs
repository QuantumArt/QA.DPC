using Quantumart.QP8.BLL;
using System;
using System.Configuration;
using System.Transactions;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using Quantumart.QP8.Constants;

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
                logger.Error("There was a try to create transaction for existing connection. Status: " + current.DbConnection.State);
            }

            var customer = connectionProvider.GetCustomer();
            _connectionScope = new QPConnectionScope(customer.ConnectionString, (DatabaseType)customer.DatabaseType);
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