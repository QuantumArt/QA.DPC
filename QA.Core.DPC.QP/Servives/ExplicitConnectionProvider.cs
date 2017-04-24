using QA.Core.DPC.QP.Models;

namespace QA.Core.DPC.QP.Servives
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
    }
}