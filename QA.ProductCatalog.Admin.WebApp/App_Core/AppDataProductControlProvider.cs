using System.IO;
using QA.Configuration;
using QA.Core.Models.Entities;
using QA.Core.Models.UI;
using QA.Core.DPC.UI;
using QA.Core;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Hosting;

namespace QA.ProductCatalog.Admin.WebApp.App_Core
{
    public class AppDataProductControlProvider : IProductControlProvider
    {
        private static readonly StackPanel stub;
        private readonly ConcurrentDictionary<string, UIElement> _cache = new ConcurrentDictionary<string, UIElement>();
        private IWebHostEnvironment _hostingEnvironment;

        static AppDataProductControlProvider()
        {
            // Этот код нужен для того, чтобы в домен приложения загрузилась сборка, в которой определен тип StackPanel
            stub = new StackPanel();
            stub.DataContext = new object();
        }

        public AppDataProductControlProvider(IWebHostEnvironment environment)
        {
            _hostingEnvironment = environment;
        }

        public UIElement GetControlForProduct(Article product)
        {
            Throws.IfArgumentNull(product, nameof(product));

            string baseDir = Path.Combine(_hostingEnvironment.WebRootPath, "/App_Data/");
            return GetXaml(Path.Combine(baseDir, $"controls/{product.ContentId}.xaml"));
        }

        protected UIElement GetXaml(string path)
        {
            if (!File.Exists(path))
            {
                Throws.IfFileNotExists(path);
            }

            return _cache.GetOrAdd($"{path}_{File.GetLastWriteTime(path).Ticks}", key => Read(path));
        }

        private static UIElement Read(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                Throws.IfNot(stream != null, "The requested file is not exist.");
                // создаем экземпляр 
                return (UIElement)XamlConfigurationParser.CreateFrom(stream);
            }
        }
    }
}
