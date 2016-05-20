using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QA.Razor.Engine;

namespace QA.Razor.Core
{
    /// <summary>
    /// Генерация результата с помощью темплейта
    /// </summary>
    public class RazorTemplateBuilder
    {
        private IViewProvider _viewProvider;

        public RazorTemplateBuilder(IViewProvider viewProvider)
        {
            if (viewProvider == null)
            {
                throw new ArgumentNullException("viewProvider");
            }

            _viewProvider = viewProvider; 
        }

        public string BuildTemplate(string templateName)
        {
            return Engine.Razor.Parse(_viewProvider.GetView(templateName));
        }

        public string BuildTemplate(string templateName, Encoding encoding)
        {
            return Engine.Razor.Parse(_viewProvider.GetView(templateName), encoding, null);
        }

        public string BuildTemplate<TModel>(string templateName, TModel model)
        {
            return Engine.Razor.Parse<TModel>(_viewProvider.GetView(templateName), model);
        }

        public string BuildTemplate<TModel>(string templateName, TModel model, Encoding encoding)
        {
            return Engine.Razor.Parse<TModel>(_viewProvider.GetView(templateName), model, encoding);
        }
    }
}
