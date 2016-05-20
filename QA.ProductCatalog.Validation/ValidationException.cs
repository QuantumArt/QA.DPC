using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.ProductCatalog.Validation
{
    [Serializable]
    public class ValidationException : Exception
    {
        public ValidationException (string message)
            : base(message)
        {
        }

		public ValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

