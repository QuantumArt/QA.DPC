using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;
using QA.Core.Models.Configuration;

namespace QA.Core.Models.Tools
{
    public static class ConfigurationSerializer
    {
        public static string GetXml<T>(T content)
        {
            return XamlServices.Save(content);
        }

        public static Content GetContent(string text)
        {
            return (Content)XamlServices.Parse(text);
        }
    }
}
