using QA.Core.DPC.QP.Models;
using System.Linq;
using System.Threading;

namespace QA.Core.DPC.QP.Services
{
    public interface IX
    {
        void Delete(string code);
        void Update(string code);
    }

    public class ConnectionWatcher
    {
        private Customer[] _customers;
        private readonly ICustomerProvider _customerProvider;
        private readonly IX[] _ix = {};
        public ConnectionWatcher(ICustomerProvider customerProvider)
        {
            _customerProvider = customerProvider;
        }

        public void Watch()
        {
            var c = _customerProvider.GetCustomers();

            foreach (var code in c)
            {             
                foreach (var x in _ix)
                {
                    Monitor.Enter(x);
                }

                foreach (var x in _ix)
                {
                    x.Update(code.CustomerCode);
                }

                foreach (var x in _ix)
                {
                    Monitor.Exit(x);
                }
            }
        }

        public void Check()
        {
            var actualCustomers = _customerProvider.GetCustomers();

            if (_customers != null)
            {
                var codes = _customers.Select(c => c.CustomerCode).ToArray();
                var actualcodes = actualCustomers.Select(c => c.CustomerCode).ToArray();
                var deletedCodes = codes.Except(actualcodes).ToArray();
                var newcodes = actualcodes.Except(actualcodes).ToArray();
                var modifiedCodes = _customers.Join(
                        actualCustomers,
                        c => c.ConnectionString,
                        c => c.ConnectionString,
                        (c, ac) => new
                        {
                            CustomerCode = c.CustomerCode,
                            Connection = c.ConnectionString,
                            ActualConnection = ac.ConnectionString
                        }
                    )
                    .Where(c => c.Connection != c.ActualConnection)
                    .Select(c => c.CustomerCode)
                    .ToArray();
            }

            _customers = actualCustomers;

        }
    }
}
