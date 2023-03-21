namespace QA.Core.DPC.Kafka.API.Interfaces;

public interface IMessageModifierFactory
{
    IMessageModifier Build(string format);
}
