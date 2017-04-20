using QA.Core.DPC.QP.Models;
using System.Collections.Generic;
using System.Configuration;

namespace QA.Core.DPC.QP.Servives
{
    public class ConnectionProvider : IConnectionProvider
    {
        private const string AdminKey = "qp_database";

        private readonly ICustomerProvider _customerProvider;
        private readonly IIdentityProvider _identityProvider;        
        private Dictionary<Service, string> _defaultConnections;
        private readonly Service _defaultService;
        private bool _qpMode;

        public ConnectionProvider(ICustomerProvider customerProvider, IIdentityProvider identityProvider, Service defaultService)
        {
            _defaultConnections = new Dictionary<Service, string>();
            _customerProvider = customerProvider;
            _identityProvider = identityProvider;
            _defaultService = defaultService;

            var qpMode =  ConfigurationManager.AppSettings["QPMode"];
            _qpMode = !string.IsNullOrEmpty(qpMode) && qpMode.ToLower() == "true";

            if (!_qpMode)
            {
                AddConnection(Service.Admin, AdminKey);
            }
        }

        private void AddConnection(Service service, string key)
        {
            _defaultConnections[service] = ConfigurationManager.ConnectionStrings[key].ConnectionString;
        }

        public string GetConnection()
        {
            return GetConnection(_defaultService);
        }
        public string GetConnection(Service service)
        {
            return _qpMode ? _customerProvider.GetConnectionString(_identityProvider.Identity.CustomerCode) : _defaultConnections[service];
        }
    }
}