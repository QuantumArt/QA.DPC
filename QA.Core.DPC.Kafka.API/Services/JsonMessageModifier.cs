using Newtonsoft.Json.Linq;
using QA.Core.DPC.Kafka.API.Interfaces;
using QA.Core.DPC.Kafka.API.Models;

namespace QA.Core.DPC.Kafka.API.Services;

public class JsonMessageModifier : IMessageModifier
{
    public string AddMethodToMessage(string message, string method)
    {
        JObject jsonMessage = JObject.Parse(message);
        jsonMessage.Add(new JProperty(InternalSettings.ActionParameterName, method));

        return jsonMessage.ToString();
    }
}
