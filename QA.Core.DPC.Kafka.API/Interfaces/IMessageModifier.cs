namespace QA.Core.DPC.Kafka.API.Interfaces;

public interface IMessageModifier
{
    string AddMethodToMessage(string message, string method);
}
