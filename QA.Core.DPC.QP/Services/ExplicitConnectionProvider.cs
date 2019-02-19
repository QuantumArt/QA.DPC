using System;
using QA.Core.DPC.QP.Models;

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

        public bool QPMode => throw new NotImplementedException();

        public bool UseQPMonitoring => throw new NotImplementedException();
        public TimeSpan TransactionTimeout => throw new NotImplementedException();
    }
}