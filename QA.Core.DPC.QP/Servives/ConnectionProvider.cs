using QA.Core.DPC.QP.Models;
using System.Collections.Generic;
using System.Configuration;

namespace QA.Core.DPC.QP.Servives
{
    public class ConnectionProvider : IConnectionProvider
    {
        private const string AdminKey = "qp_database";
        private const string NotificationKey = "QA.Core.DPC.Properties.Settings.beeline_dpc_notificationsConnectionString";

        private readonly ICustomerProvider _customerProvider;
        private readonly IIdentityProvider _identityProvider;        
        private Dictionary<Service, string> _defaultConnections;
        private readonly Service _defaultService;
        public bool QPMode { get; private set; }

        public ConnectionProvider(ICustomerProvider customerProvider, IIdentityProvider identityProvider, Service defaultService)
        {
            _defaultConnections = new Dictionary<Service, string>();
            _customerProvider = customerProvider;
            _identityProvider = identityProvider;
            _defaultService = defaultService;

            QPMode = GetQPMode();

            if (!QPMode)
            {
                AddConnection(Service.Admin, AdminKey);
                AddConnection(Service.Notification, NotificationKey);
            }
        }

        public static bool GetQPMode()
        {
            var qpMode = ConfigurationManager.AppSettings["QPMode"];
            return !string.IsNullOrEmpty(qpMode) && qpMode.ToLower() == "true";
        }

        private void AddConnection(Service service, string key)
        {
            var connection = ConfigurationManager.ConnectionStrings[key];

            if (connection != null)
            {
                _defaultConnections[service] = connection.ConnectionString;
            }
        }

        public bool HasConnection(Service service)
        {
            return _defaultConnections.ContainsKey(service);
        }

        public string GetConnection()
        {
            return GetConnection(_defaultService);
        }
        public string GetConnection(Service service)
        {
            return QPMode ? _customerProvider.GetConnectionString(_identityProvider.Identity.CustomerCode) : _defaultConnections[service];
        }
    }
}