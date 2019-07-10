
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
        private readonly Dictionary<Service, Customer> _defaultConnections;
        private readonly Service _defaultService;
        private readonly ConnectionProperties _cnnProps;
        public bool QPMode => _cnnProps.QpMode || _defaultService == Service.HighloadAPI;
        
        public bool UseQPMonitoring => _cnnProps.UseQpMonitoring;

        public bool UsePostgres => _cnnProps.UsePostgres;

        public TimeSpan TransactionTimeout => _cnnProps.TransactionTimeout;

        public CoreConnectionProvider(ICustomerProvider customerProvider, 
            IIdentityProvider identityProvider, 
            IOptions<ConnectionProperties> cnnProps,

            Service defaultService = Service.Admin)
        {
            _defaultConnections = new Dictionary<Service, Customer>();
            _customerProvider = customerProvider;
            _identityProvider = identityProvider;
            _defaultService = defaultService;
            _cnnProps = cnnProps.Value;
            if (!QPMode)
            {
                var dbType = _cnnProps.GetDatabaseType();
                _defaultConnections.Add(Service.Admin, new Customer{ 
                    ConnectionString = _cnnProps.DpcConnectionString,
                    DatabaseType = dbType});
                _defaultConnections.Add(Service.Actions, new Customer{ 
                    ConnectionString = _cnnProps.TasksConnectionString,
                    DatabaseType = dbType});
                _defaultConnections.Add(Service.Notification, new Customer{ 
                    ConnectionString = _cnnProps.NotificationsConnectionString,
                    DatabaseType = dbType});
            }
        }

        public bool HasConnection(Service service)
        {
            return _defaultConnections.ContainsKey(service);
        }

        [Obsolete("This method doesn't provide database type information. Use method GetCustomer instead.")]
        public string GetConnection()
        {
            return GetConnection(_defaultService);
        }
        
        [Obsolete("This method doesn't provide database type information. Use method GetCustomer instead.")]
        public string GetConnection(Service service)
        {
            return QPMode ? _customerProvider.GetConnectionString(_identityProvider.Identity.CustomerCode) : _defaultConnections[service].ConnectionString;
        }

        public Customer GetCustomer()
        {
            return GetCustomer(_defaultService);
        }
        
        public Customer GetCustomer(Service service)
        {
            return QPMode ? _customerProvider.GetCustomer(_identityProvider.Identity.CustomerCode) : _defaultConnections[service];
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