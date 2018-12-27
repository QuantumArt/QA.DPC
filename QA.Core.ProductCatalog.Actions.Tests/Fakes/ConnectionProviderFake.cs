using System;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;

namespace QA.Core.ProductCatalog.Actions.Tests.Fakes
{
    public class ConnectionProviderFake : IConnectionProvider
    {
        private readonly string _connection;

        public ConnectionProviderFake(string connection)
        {
            _connection = connection;
        }

        public bool QPMode => throw new NotImplementedException();

        public bool UseQPMonitoring => throw new NotImplementedException();

        public string GetConnection()
        {
            return _connection;
        }

        public string GetConnection(DPC.QP.Models.Service service)
        {
            return _connection;
        }

        public string GetEFConnection()
        {
            throw new NotImplementedException();
        }

        public string GetEFConnection(DPC.QP.Models.Service service)
        {
            throw new NotImplementedException();
        }

        public bool HasConnection(DPC.QP.Models.Service service)
        {
            throw new NotImplementedException();
        }
    }
}
