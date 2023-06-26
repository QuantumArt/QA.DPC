using System;

namespace QA.ProductCatalog.HighloadFront.Exceptions
{
    public abstract class BaseLocalizedException : Exception
    {
        public string LocalizedMessage { get; init; }

        public BaseLocalizedException(string message, string localizedMessage)
            : base(message)
        {
            LocalizedMessage = localizedMessage;
        }
    }
}
