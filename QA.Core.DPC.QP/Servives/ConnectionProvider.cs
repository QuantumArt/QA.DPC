using System.Configuration;

namespace QA.Core.DPC.QP.Servives
{
    public class ConnectionProvider : IConnectionProvider
    {
        private readonly ICustomerProvider _customerProvider;
        private readonly IIdentityProvider _identityProvider;
        private string _defaultConnection = null;

        public ConnectionProvider(ICustomerProvider customerProvider, IIdentityProvider identityProvider)
        {
            _customerProvider = customerProvider;
            _identityProvider = identityProvider;

            var qpMode =  ConfigurationManager.AppSettings["QPMode"];

            if (!string.IsNullOrEmpty(qpMode) && qpMode.ToLower() == "true")
            {
                _defaultConnection = ConfigurationManager.ConnectionStrings["qp_database"].ConnectionString;
            }
        }

        public string GetConnection()
        {
            return _defaultConnection ?? _customerProvider.GetConnectionString(_identityProvider.Identity.CustomerCode);         
        }   
    }
}