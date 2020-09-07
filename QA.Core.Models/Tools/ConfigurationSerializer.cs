using Portable.Xaml;
using QA.Configuration;
using QA.Core.Models.Configuration;

namespace QA.Core.Models.Tools
{
    public static class ConfigurationSerializer
    {
        public static string GetXml<T>(T content)
        {
            return XamlConfigurationParser.Save(content);
        }

        public static Content GetContent(string text)
        {
            return (Content)XamlConfigurationParser.CreateFrom(text);
        }
    }
}
