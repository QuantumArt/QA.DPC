using QA.Core.DPC.QP.Models;

namespace QA.Core.DPC.QP.Services
{
    public interface IConnectionProvider
    {
        string GetConnection();
        string GetEFConnection();
        string GetConnection(Service service);
        string GetEFConnection(Service service);
        bool HasConnection(Service service);
        bool QPMode { get; }
    }
}