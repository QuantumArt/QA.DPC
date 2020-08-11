using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using Portable.Xaml;

namespace QA.Core.DPC.QP.API.Tests.Providers
{
    public abstract class ProviderBase
    {
        public static T GetXaml<T>(string path)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path))
            {
                return (T)XamlServices.Load(stream);
            }
        }

        public static void SaveXaml(string path, object data)
        {
            XamlServices.Save(data);
        }

        public static T GetJson<T>(string path)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path))
            using (var streamReader = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<T>(jsonTextReader);
            }
        }
    }
}
