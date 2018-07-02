using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public string GetConnection(Service service)
        {
            return _connection;
        }

        public string GetEFConnection()
        {
            throw new NotImplementedException();
        }

        public string GetEFConnection(Service service)
        {
            throw new NotImplementedException();
        }

        public bool HasConnection(Service service)
        {
            throw new NotImplementedException();
        }
    }
}
