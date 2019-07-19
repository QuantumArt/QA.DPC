using System;
using QA.Core.DPC.QP.Models;

namespace QA.Core.DPC.QP.Services
{
    public interface IConnectionProvider
    {
        string GetConnection();
        string GetEFConnection();
        string GetConnection(Service service);
        string GetEFConnection(Service service);
        Customer GetCustomer();
        Customer GetCustomer(Service service);
        
        bool HasConnection(Service service);
        bool QPMode { get; }
        bool UseQPMonitoring { get; }
        TimeSpan TransactionTimeout { get;  }
    }
}