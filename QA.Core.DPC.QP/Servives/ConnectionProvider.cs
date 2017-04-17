using System.Configuration;

namespace QA.Core.DPC.QP.Servives
{
    public class ConnectionProvider : IConnectionProvider
    {
        private readonly ICustomerProvider _customerProvider;
        private readonly ICustomerCodeProvider _codeProvider;
        private string _defaultConnection = null;

        public ConnectionProvider(ICustomerProvider customerProvider, ICustomerCodeProvider codeProvider)
        {
            _customerProvider = customerProvider;
            _codeProvider = codeProvider;

            var qpMode =  ConfigurationManager.AppSettings["QPMode"];

            if (!string.IsNullOrEmpty(qpMode) && qpMode.ToLower() == "true")
            {
                _defaultConnection = ConfigurationManager.ConnectionStrings["qp_database"].ConnectionString;
            }
        }

        public string GetConnection()
        {
            return _defaultConnection ?? _customerProvider.GetConnectionString(_codeProvider.CustomerCode);         
        }   
    }
}