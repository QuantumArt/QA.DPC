
using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Extensions.Options;
using QA.Core.DPC.QP.Models;
using QP.ConfigurationService.Models;

namespace QA.Core.DPC.QP.Services
{
    public class CoreConnectionProvider : IConnectionProvider
    {
        private readonly ICustomerProvider _customerProvider;
        private readonly IIdentityProvider _identityProvider;        
        private readonly Dictionary<Models.Service, Customer> _defaultConnections;
        private readonly Models.Service _defaultService;
        private readonly ConnectionProperties _cnnProps;
        public bool QPMode => _cnnProps.QpMode || _defaultService == Models.Service.HighloadAPI;
        
        public bool UseQPMonitoring => _cnnProps.UseQpMonitoring;

        public bool UsePostgres => _cnnProps.UsePostgres;

        public TimeSpan TransactionTimeout => _cnnProps.TransactionTimeout;

        public CoreConnectionProvider(ICustomerProvider customerProvider, 
            IIdentityProvider identityProvider, 
            IOptions<ConnectionProperties> cnnProps,

            Models.Service defaultService = Models.Service.Admin)
        {
            _defaultConnections = new Dictionary<Models.Service, Customer>();
            _customerProvider = customerProvider;
            _identityProvider = identityProvider;
            _defaultService = defaultService;
            _cnnProps = cnnProps.Value;
            if (!QPMode)
            {
                var dbType = _cnnProps.GetDatabaseType();
                _defaultConnections.Add(Models.Service.Admin, new Customer{ 
                    ConnectionString = _cnnProps.DpcConnectionString,
                    DatabaseType = dbType});
                _defaultConnections.Add(Models.Service.Actions, new Customer{ 
                    ConnectionString = _cnnProps.TasksConnectionString,
                    DatabaseType = dbType});
                _defaultConnections.Add(Models.Service.Notification, new Customer{ 
                    ConnectionString = _cnnProps.NotificationsConnectionString,
                    DatabaseType = dbType});
            }
        }

        public bool HasConnection(Models.Service service)
        {
            return _defaultConnections.ContainsKey(service);
        }

        [Obsolete("This method doesn't provide database type information. Use method GetCustomer instead.")]
        public string GetConnection()
        {
            return GetConnection(_defaultService);
        }
        
        [Obsolete("This method doesn't provide database type information. Use method GetCustomer instead.")]
        public string GetConnection(Models.Service service)
        {
            return QPMode ? _customerProvider.GetConnectionString(_identityProvider.Identity.CustomerCode) : _defaultConnections[service].ConnectionString;
        }

        public Customer GetCustomer()
        {
            return GetCustomer(_defaultService);
        }
        
        public Customer GetCustomer(Models.Service service)
        {
            return QPMode ? _customerProvider.GetCustomer(_identityProvider.Identity.CustomerCode) : _defaultConnections[service];
        }
        
        [Obsolete]
        public string GetEFConnection()
        {
            return GetEFConnection(_defaultService);
        }

        [Obsolete]
        public string GetEFConnection(Models.Service service)
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