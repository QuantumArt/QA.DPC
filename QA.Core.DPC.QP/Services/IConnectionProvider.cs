using System;
using QA.Core.DPC.QP.Models;

namespace QA.Core.DPC.QP.Services
{
    public interface IConnectionProvider
    {
        string GetConnection();
        string GetEFConnection();
        string GetConnection(Models.Service service);
        string GetEFConnection(Models.Service service);
        Customer GetCustomer();
        Customer GetCustomer(Models.Service service);
        
        bool HasConnection(Models.Service service);
        bool QPMode { get; }
        bool UseQPMonitoring { get; }
        TimeSpan TransactionTimeout { get;  }
    }
}