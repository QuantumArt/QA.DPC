using System;
using System.Runtime.Serialization;

namespace QA.Core.DPC.QP.Exceptions
{
    public class ConsolidationException : Exception
    {
        public ConsolidationException()
            : base()
        {

        }

        public ConsolidationException(string message)
            : base(message)
        {

        }

        public ConsolidationException(string message, Exception innerException)
          : base(message, innerException)
        {

        }

        public ConsolidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
