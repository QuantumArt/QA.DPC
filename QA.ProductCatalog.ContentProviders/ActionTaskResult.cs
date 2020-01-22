using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QA.ProductCatalog.ContentProviders
{
    public class ActionTaskResult
    {

        public static ActionTaskResult Success(string message)
        {
            return Success(new ActionTaskResultMessage { Message = message});
        }
        
        public static ActionTaskResult Success(ActionTaskResultMessage message)
        {
            var result = new ActionTaskResult {IsSuccess = true};
            result.Messages.Add(message);
            return result;
        }
        
        public static ActionTaskResult PartialSuccess(ActionTaskResultMessage message, int[] failedIds)
        {
            var result = new ActionTaskResult {IsSuccess = true, FailedIds = failedIds};
            result.Messages.Add(message);
            return result;
        }

        public static ActionTaskResult Error(ActionTaskResultMessage message, int[] failedIds)
        {
            var result = new ActionTaskResult {IsSuccess = false, FailedIds = failedIds};
            result.Messages.Add(message);
            return result;
        }
        
        public static ActionTaskResult Error(string message)
        {
            return Error(new ActionTaskResultMessage() { Message = message}, null);
        }
        
        public bool IsSuccess { get; set; }

        public int[] FailedIds { get; set; } = { };
        
        public List<ActionTaskResultMessage> Messages { get; } = new List<ActionTaskResultMessage>();

        public override string ToString()
        {
            return Messages.Any() 
                ? string.Join(". ", Messages.Select(n => n.ToString())) 
                : String.Empty;
        }
    }
}