using System;
using QA.Core.DPC.QP.Models;
using QP.ConfigurationService.Models;

namespace QA.Core.DPC.QP.Services
{
    public class ExplicitConnectionProvider : IConnectionProvider
    {
        private readonly Customer _customer;
     
        public ExplicitConnectionProvider(string connection, DatabaseType dbType = DatabaseType.SqlServer)
        {
            _customer = new Customer
            {
                ConnectionString = connection,
                DatabaseType = dbType
            };
        }
        
        public ExplicitConnectionProvider(Customer customer)
        {
            _customer = customer;
        }
        

        public string GetConnection()
        {
            return _customer.ConnectionString;
        }
        public string GetConnection(Service service)
        {
            return _customer.ConnectionString;
        }

        public Customer GetCustomer(Service service)
        {
            return _customer;
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
            return _customer;
        }

        public bool QPMode => throw new NotImplementedException();
        public bool UseQPMonitoring => throw new NotImplementedException();
        public TimeSpan TransactionTimeout => throw new NotImplementedException();
    }
}