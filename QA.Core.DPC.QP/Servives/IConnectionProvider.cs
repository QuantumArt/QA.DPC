using QA.Core.DPC.QP.Models;

namespace QA.Core.DPC.QP.Servives
{
    public interface IConnectionProvider
    {
        string GetConnection();
        string GetConnection(Service service);
    }
}