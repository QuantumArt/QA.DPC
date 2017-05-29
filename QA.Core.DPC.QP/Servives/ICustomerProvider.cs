using QA.Core.DPC.QP.Models;

namespace QA.Core.DPC.QP.Servives
{
    public interface ICustomerProvider
    {
        string GetConnectionString(string customerCode);
        Customer[] GetCustomers();
    }
}
