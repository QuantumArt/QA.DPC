using System.Xml.Linq;
using QA.Core.DPC.Kafka.API.Interfaces;
using QA.Core.DPC.Kafka.API.Models;

namespace QA.Core.DPC.Kafka.API.Services;

public class XmlMessageModifier : IMessageModifier
{
    public string AddMethodToMessage(string message, string method)
    {
        XDocument xmlMessage = XDocument.Parse(message);
        XElement rootElement = xmlMessage.Root;

        if (rootElement == null)
        {
            throw new InvalidOperationException("Xml/Xaml was parsed into null object.");
        }

        rootElement.Add(new XElement(InternalSettings.ActionParameterName, method));
        
        return xmlMessage.ToString();
    }
}
