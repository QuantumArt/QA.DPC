using System;

namespace QA.ProductCatalog.HighloadFront.Exceptions
{
    public class NamedPropertyBusyExpandException : Exception
    {
        public string LocalizedMessage { get; init; }

        public NamedPropertyBusyExpandException(string message, string localizedMessage)
            : base(message)
        {
            LocalizedMessage = localizedMessage;
        }
    }
}
