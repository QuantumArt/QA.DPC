using System;
using QA.ProductCatalog.ContentProviders;

namespace QA.ProductCatalog.Validation
{
    [Serializable]
    public class ValidationException : Exception
    {
        public ActionTaskResultMessage ResultMessage { get; }
        
        public ValidationException (ActionTaskResultMessage message)
            : base(message.ResourceName)
        {
            ResultMessage = message;
        }

		public ValidationException(ActionTaskResultMessage message, Exception innerException)
            : base(message.ResourceName, innerException)
        {
            ResultMessage = message;
        }
    }
}

