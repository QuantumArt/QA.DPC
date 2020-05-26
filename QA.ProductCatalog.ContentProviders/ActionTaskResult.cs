using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Quantumart.QP8.BLL;

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

        public static ActionTaskResult Error(ActionTaskResultMessage message, int[] failedIds = null)
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

        public static ActionTaskResult FromString(string str)
        {
            try
            {
                return JsonConvert.DeserializeObject<ActionTaskResult>(str);
            }
            catch
            {
                return null;
            }
        }

        public override string ToString()
        {
            return Messages.Any() 
                ? string.Join(". ", Messages.Select(n => n.ToString())) 
                : String.Empty;
        }
        
        public static ActionTaskResult FromRulesException(RulesException ex, int id)
        {
            var result = new ActionTaskResult() { IsSuccess = true };
            if (!ex.IsEmpty)
            {
                result.IsSuccess = false;
                var messages = ex.Errors
                    .Select(s => ActionTaskResultMessage.FromString(s.Message)).ToArray();
                
                foreach (var message in messages)
                {
                    message.Extra = id.ToString();
                    result.Messages.Add(message);
                }
            }
            return result;
        }

        public ActionTaskResult GetMergedResult()
        {
            var dict = new Dictionary<string, ActionTaskResultMessage>();
            var result = new ActionTaskResult() {IsSuccess = IsSuccess, FailedIds = FailedIds.ToArray()};
            foreach (var message in Messages)
            {
                if (message.ResourceName != null || message.Message != null)
                {
                    var key = message.ResourceName ?? message.Message;
                    if (message.Parameters != null && message.Parameters.Any())
                    {
                        key += "=" + string.Join(",",  message.Parameters);
                    }

                    if (!string.IsNullOrEmpty(message.Extra) && dict.TryGetValue(key, out var value))
                    {
                        value.Extra += ", " + message.Extra;
                    }
                    else
                    {
                        dict.Add(key, message);
                    }
                }
                else
                {
                    result.Messages.Add(message);
                }
            }

            result.Messages.AddRange(dict.Values);
 
            return result;
        }
    }
}