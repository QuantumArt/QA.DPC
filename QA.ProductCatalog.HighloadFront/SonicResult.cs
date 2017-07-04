using System;
using System.Collections.Generic;
using System.Linq;
using QA.ProductCatalog.HighloadFront.Models;

namespace QA.ProductCatalog.HighloadFront
{
    public class SonicResult
    {
        private List<SonicError> _errors = new List<SonicError>();
        public bool Succeeded { get; protected set; }
        public IEnumerable<SonicError> Errors => _errors;
        public static SonicResult Success { get; } = new SonicResult { Succeeded = true };

        public static SonicResult Failed(params SonicError[] errors)
        {
            var result = new SonicResult { Succeeded = false };
            if (errors != null)
            {
                result._errors.AddRange(errors);
            }
            return result;
        }
        
        public Exception GetException()
        {
             var exceptions = Errors.Select(e => e.Exception).Where(e => e != null).ToArray();

            if (exceptions.Length == 0)
            {
                return null;
            }
            else if (exceptions.Length == 1)
            {
                return new Exception(ToString(), exceptions[0]);
            }
            else
            {
                return new AggregateException(ToString(), exceptions);
            }
        }
        public override string ToString()
        {
            return Succeeded
                ? "Succeeded"
                : $"Failed with: {string.Join(",", Errors.Select(x => x.Code + " : " + x.Description).ToList())}.";
        }
    }
}