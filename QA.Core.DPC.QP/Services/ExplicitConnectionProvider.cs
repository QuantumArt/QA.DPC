using System;
using QA.Core.DPC.QP.Models;
using QP.ConfigurationService.Models;

namespace QA.Core.DPC.QP.Services
{
    public class ExplicitConnectionProvider : IConnectionProvider
    {     
        private readonly string _connection;
     
        public ExplicitConnectionProvider(string connection)
        {
            _connection = connection;
        }

        public string GetConnection()
        {
            return _connection;
        }
        public string GetConnection(Service service)
        {
            return _connection;
        }

        public Customer GetCustomer(Service service)
        {
            return new Customer
            {
                ConnectionString = _connection,
                DatabaseType = UsePostgres ? DatabaseType.Postgres : DatabaseType.SqlServer
            };
        }

        public bool HasConnection(Service service)
        {
            throw new NotImplementedException();
        }

        public string GetEFConnection()
        {
            throw new NotImplementedException();
        }

        public string GetEFConnection(Service service)
        {
            throw new NotImplementedException();
        }

        public Customer GetCustomer()
        {
            return new Customer
            {
                ConnectionString = _connection,
                DatabaseType = UsePostgres ? DatabaseType.Postgres : DatabaseType.SqlServer
            };
        }

        public bool QPMode => throw new NotImplementedException();
        public bool UsePostgres { get; }

        public bool UseQPMonitoring => throw new NotImplementedException();
        public TimeSpan TransactionTimeout => throw new NotImplementedException();
    }
}