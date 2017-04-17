using System.Web;

namespace QA.Core.DPC.QP.Servives
{
    public class HttpContextCustomerCodeProvider : ICustomerCodeProvider
    {
        private const string CustomerCodeKey = "CustomerCode";
        public string CustomerCode
        {
            get
            {
                return HttpContext.Current.Items[CustomerCodeKey] as string;
            }

            set
            {
                HttpContext.Current.Items[CustomerCodeKey] = value;
            }
        }
    }
}
