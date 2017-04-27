using QA.Core.DPC.QP.Servives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.DPC.QP.Models;

namespace QA.Core.ProductCatalog.Actions.Tests.Fakes
{
    public class ConnectionProviderFake : IConnectionProvider
    {
        private readonly string _connection;

        public ConnectionProviderFake(string connection)
        {
            _connection = connection;
        }

        public bool QPMode
        {
            get
            {
                throw new NotImplementedException();
            }
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
    }
}
