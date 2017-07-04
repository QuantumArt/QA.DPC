using QA.Core.DPC.QP.Models;

namespace QA.Core.DPC.QP.Services
{
    public interface ICustomerProvider
    {
        string GetConnectionString(string customerCode);
        Customer[] GetCustomers();
    }
}
