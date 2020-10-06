using System.IO;
using System.Reflection;
using QA.Configuration;
using QA.Core.Models.Entities;
using QA.Core.Models.UI;
using QA.Core.DPC.UI;

namespace QA.Core.DPC.Xaml
{
    public class ProductControlProvider : IProductControlProvider
    {
        private static readonly StackPanel stub;

        static ProductControlProvider()
        {
            // Этот код нужен для того, чтобы в домен приложения загрузилась сборка, в которой определен тип StackPanel
            stub = new StackPanel();
        }

        public UIElement GetControlForProduct(Article product)
        {
            stub.DataContext = this;
            return GetXaml<UIElement>(string.Format("QA.Core.DPC.Xaml.Xaml.{0}.xaml", product.ContentId));
        }

        protected T GetXaml<T>(string path)
        {
            using (var stream = Assembly.GetExecutingAssembly()
               .GetManifestResourceStream(path))
            {
                Throws.IfNot(stream != null, "The requested file is not found in embedded resource.");
                // создаем экземпляр 
                return (T)XamlConfigurationParser.LoadFrom(stream);
            }
        }

        protected virtual string GetXamlText(string path)
        {
            using (var stream = Assembly.GetExecutingAssembly()
               .GetManifestResourceStream(path))
            {
                using (var textReader = new StreamReader(stream))
                {
                    return textReader.ReadToEnd();
                }
            }
        }

    }
}
