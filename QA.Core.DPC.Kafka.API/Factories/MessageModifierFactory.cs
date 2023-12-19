using QA.Core.DPC.Kafka.API.Interfaces;
using QA.Core.DPC.Kafka.API.Services;

namespace QA.Core.DPC.Kafka.API.Factories;

public class MessageModifierFactory : IMessageModifierFactory
{
    private readonly JsonMessageModifier _jsonModifier;
    private readonly XmlMessageModifier _xmlModifier;
    public MessageModifierFactory(JsonMessageModifier jsonModifier, XmlMessageModifier xmlModifier)
    {
        _jsonModifier = jsonModifier;
        _xmlModifier = xmlModifier;
    }

    public IMessageModifier Build(string format)
    {
        switch (format)
        {
            case "json":
                return _jsonModifier;
            case "xml":
            case "xaml":
                return _xmlModifier;
            default:
                throw new InvalidOperationException($"Unsupported format: {format}.");
        }
    }
}
