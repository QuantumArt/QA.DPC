using System;
using System.Collections.Generic;
using System.Configuration;
using QA.Core.DPC.QP.Models;

namespace QA.Core.DPC.QP.Services
{
    public class ConnectionProvider : IConnectionProvider
    {
        private const string AdminKey = "qp_database";
        private const string NotificationKey = "QA.Core.DPC.Properties.Settings.notification_database";
        private const string ActionsKey = "TaskRunnerEntities";

        private readonly ICustomerProvider _customerProvider;
        private readonly IIdentityProvider _identityProvider;        
        private Dictionary<Service, string> _defaultConnections;
        private readonly Service _defaultService;
        public bool QPMode { get; private set; }
        public bool UseQPMonitoring { get; private set; }

        public ConnectionProvider(ICustomerProvider customerProvider, IIdentityProvider identityProvider, Service defaultService)
        {
            _defaultConnections = new Dictionary<Service, string>();
            _customerProvider = customerProvider;
            _identityProvider = identityProvider;
            _defaultService = defaultService;

            QPMode = GetQPMode() || defaultService == Service.HighloadAPI;
            UseQPMonitoring = GetUseQPMonitoring();

            if (!QPMode)
            {
                AddConnection(Service.Admin, AdminKey);
                AddConnection(Service.Notification, NotificationKey);
                AddConnection(Service.Actions, ActionsKey);
            }
        }

        public static bool GetQPMode()
        {
            var qpMode = ConfigurationManager.AppSettings["QPMode"];
            return !string.IsNullOrEmpty(qpMode) && qpMode.ToLower() == "true";
        }

        public static bool GetUseQPMonitoring()
        {
            var useQpMonitoring = ConfigurationManager.AppSettings["UseQPMonitoring"];
            return !string.IsNullOrEmpty(useQpMonitoring) && useQpMonitoring.ToLower() == "true";
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

        public string GetEFConnection()
        {
            return GetEFConnection(_defaultService);
        }

        public string GetEFConnection(Service service)
        {
            var connection = GetConnection(service);

            if (!IsEFConnection(connection))
            {
                connection = ConvertToEFConnection(connection);
            }

            return connection;
        }

        private string ConvertToEFConnection(string connectionString)
        {
            return $"metadata=res://*/Model.csdl|res://*/Model.ssdl|res://*/Model.msl;provider=System.Data.SqlClient;provider connection string=\"{connectionString}\"";
        }

        private bool IsEFConnection(string connectionString)
        {
            return connectionString.StartsWith("metadata", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}