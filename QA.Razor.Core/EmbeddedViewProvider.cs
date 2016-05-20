// Owners: Karlov Nikolay, Abretov Alexey

using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace QA.Razor.Core
{
    public abstract class EmbeddedViewProvider : IViewProvider
    {
        public abstract string[] ViewPath { get; }

        public TemplateContent GetView(string viewName)
        {
            if (string.IsNullOrWhiteSpace(viewName))
            {
                throw new ArgumentNullException(viewName);
            }

            string template = null;

            Assembly asm = Assembly.GetEntryAssembly();

            var paths = ViewPath.Select(
                pathFormat => string
                    .Format(pathFormat, asm.GetName().Name, viewName));

            // выбираем первый найденный шаблон
            foreach (var path in paths)
            {
                using (Stream s = asm.GetManifestResourceStream(path))
                {
                    if (s == null)
                    {
                        continue;
                    }

                    using (var sr = new StreamReader(s))
                    {
                        template = sr.ReadToEnd();

                        if (template != null)
                        {
                            break;
                        }
                    }
                }
            }

            if (template == null)
            {
                throw new Exception("Не найден шаблон: " + viewName);
            }

            return new TemplateContent(template);
        }
    }
}
