
using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Extensions.Options;
using QA.Core.DPC.QP.Models;

namespace QA.Core.DPC.QP.Services
{
    public class CoreConnectionProvider : IConnectionProvider
    {
        private readonly ICustomerProvider _customerProvider;
        private readonly IIdentityProvider _identityProvider;        
        private Dictionary<Service, string> _defaultConnections;
        private readonly Service _defaultService;
        private readonly ConnectionProperties _cnnProps;
        public bool QPMode => _cnnProps.QpMode || _defaultService == Service.HighloadAPI;
        
        public bool UseQPMonitoring => _cnnProps.UseQpMonitoring;

        public bool UsePostgres => _cnnProps.UsePostgres;

        public TimeSpan TransactionTimeout => _cnnProps.TransactionTimeout;

        public CoreConnectionProvider(ICustomerProvider customerProvider, IIdentityProvider identityProvider, IOptions<ConnectionProperties> cnnProps, Service defaultService = Service.Admin)
        {
            _defaultConnections = new Dictionary<Service, string>();
            _customerProvider = customerProvider;
            _identityProvider = identityProvider;
            _defaultService = defaultService;
            _cnnProps = cnnProps.Value;

            if (!QPMode)
            {
                _defaultConnections.Add(Service.Admin, _cnnProps.DpcConnectionString);
                _defaultConnections.Add(Service.Actions, _cnnProps.TasksConnectionString);
                _defaultConnections.Add(Service.Notification, _cnnProps.NotificationsConnectionString);            
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