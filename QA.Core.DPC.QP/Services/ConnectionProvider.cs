using System;
using System.Collections.Generic;
using System.Configuration;
using QA.Core.DPC.QP.Models;
using QP.ConfigurationService.Models;

namespace QA.Core.DPC.QP.Services
{
    public class ConnectionProvider : IConnectionProvider
    {
        private const string AdminKey = "qp_database";
        private const string NotificationKey = "QA.Core.DPC.Properties.Settings.notification_database";
        private const string ActionsKey = "TaskRunnerEntities";

        private readonly ICustomerProvider _customerProvider;
        private readonly IIdentityProvider _identityProvider;        
        private Dictionary<Models.Service, Customer> _defaultConnections;
        private readonly Models.Service _defaultService;
        public bool QPMode { get; private set; }
        public bool UsePostgres { get; }
        public bool UseQPMonitoring { get; private set; }
        
        public TimeSpan TransactionTimeout { get; private set; }

        public ConnectionProvider(ICustomerProvider customerProvider, IIdentityProvider identityProvider, Models.Service defaultService)
        {
            _defaultConnections = new Dictionary<Models.Service, Customer>();
            _customerProvider = customerProvider;
            _identityProvider = identityProvider;
            _defaultService = defaultService;

            QPMode = GetQPMode() || defaultService == Models.Service.HighloadAPI;
            UseQPMonitoring = GetUseQPMonitoring();
            TransactionTimeout = GetTransactionTimeout();

            if (!QPMode)
            {
                AddConnection(Models.Service.Admin, AdminKey);
                AddConnection(Models.Service.Notification, NotificationKey);
                AddConnection(Models.Service.Actions, ActionsKey);
            }
        }

        public bool GetQPMode()
        {
            var qpMode = ConfigurationManager.AppSettings["QPMode"];
            return !string.IsNullOrEmpty(qpMode) && qpMode.ToLower() == "true";
        }

        public bool GetUseQPMonitoring()
        {
            var useQpMonitoring = ConfigurationManager.AppSettings["UseQPMonitoring"];
            return !string.IsNullOrEmpty(useQpMonitoring) && useQpMonitoring.ToLower() == "true";
        }

        public TimeSpan GetTransactionTimeout()
        {
            TimeSpan timeout;
            string configTimeout = ConfigurationManager.AppSettings["ProductCatalog.Actions.TransactionTimeout"];

            if (!TimeSpan.TryParse(configTimeout, out timeout))
            {
                timeout = TimeSpan.FromMinutes(3);
            }

            return timeout;
        }

        private void AddConnection(Models.Service service, string key)
        {
            var connection = ConfigurationManager.ConnectionStrings[key];

            if (connection != null)
            {
                _defaultConnections[service] = new Customer(
                    connection.ConnectionString, 
                    string.Empty,
                    UsePostgres ? DatabaseType.Postgres : DatabaseType.SqlServer
                );
            }
        }

        public Customer GetCustomer(Models.Service service)
        {
            return QPMode ? _customerProvider.GetCustomer(_identityProvider.Identity.CustomerCode) : _defaultConnections[service];
        }

        public bool HasConnection(Models.Service service)
        {
            return _defaultConnections.ContainsKey(service);
        }

        public string GetConnection()
        {
            return GetConnection(_defaultService);
        }
        public string GetConnection(Models.Service service)
        {
            return QPMode ? _customerProvider.GetConnectionString(_identityProvider.Identity.CustomerCode) : _defaultConnections[service].ConnectionString;
        }

        public string GetEFConnection()
        {
            return GetEFConnection(_defaultService);
        }

        public string GetEFConnection(Models.Service service)
        {
            var connection = GetConnection(service);

            if (!IsEFConnection(connection))
            {
                connection = ConvertToEFConnection(connection);
            }

            return connection;
        }

        public Customer GetCustomer()
        {
            return GetCustomer(_defaultService);
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