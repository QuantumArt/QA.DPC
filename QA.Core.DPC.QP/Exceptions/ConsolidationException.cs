using System;

namespace QA.Core.DPC.QP.Exceptions
{
    public class ConsolidationException : Exception
    {
        public ConsolidationException(string message)
            : base(message)
        {

        }

        public ConsolidationException(string message, Exception innerException)
          : base(message, innerException)
        {

        }
    }
}
