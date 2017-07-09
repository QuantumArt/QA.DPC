﻿using System;
using System.Data.SqlClient;
using System.Linq;
using QA.Core.DPC.QP.Models;
using Quantumart.QP8.Configuration;
using Quantumart.QPublishing.Database;

namespace QA.Core.DPC.QP.Services
{
    public class CustomerProvider : ICustomerProvider
    {
        private const int Timeout = 2;
        private readonly QPConfiguration _configuration;
        private readonly ILogger _logger;

        public CustomerProvider(ILogger logger)
        {
            _configuration = new QPConfiguration();
            _logger = logger;
        }

        public string GetConnectionString(string customerCode)
        {
            return QPConfiguration.GetConnectionString(customerCode);
        }

        public Customer[] GetCustomers()
        {
            return QPConfiguration.CustomerCodes
                .Select(c => new Customer
                {
                    ConnectionString = QPConfiguration.GetConnectionString(c),
                    CustomerCode = c
                })
                .Where(customer => IsDPCMode(customer))
                .ToArray();
        }

        public bool IsDPCMode(Customer customer)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(customer.ConnectionString);
                builder.ConnectTimeout = Timeout;
                var connector = new DBConnector(builder.ConnectionString);
                var command = new SqlCommand("SELECT USE_DPC FROM DB");
                return (bool)connector.GetRealScalarData(command);
            }
            catch(Exception)
            {
                _logger.LogError(() => $"Customer code {customer.CustomerCode} is not accessible");
                return false;
            }
        }
    }
}