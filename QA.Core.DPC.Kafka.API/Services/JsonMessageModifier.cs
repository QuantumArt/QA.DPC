using System.Text.Json.Nodes;
using QA.Core.DPC.Kafka.API.Interfaces;
using QA.Core.DPC.Kafka.API.Models;

namespace QA.Core.DPC.Kafka.API.Services;

public class JsonMessageModifier : IMessageModifier
{
    public string AddMethodToMessage(string message, string method)
    {
        var jsonNode = JsonNode.Parse(message);
        if (jsonNode == null) throw new ArgumentException(null, nameof(message));
        jsonNode[InternalSettings.ActionParameterName] = method;
        return jsonNode.ToJsonString();
   }
}
