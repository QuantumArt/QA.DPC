using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QA.Razor.Core
{
    /// <summary>
    /// Razor-шаблон
    /// </summary>
    public class TemplateContent
    {
        /// <summary>
        /// Текст шаблона
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Создает экземпляр шаблона
        /// </summary>
        /// <param name="content"></param>
        public TemplateContent(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentNullException("content");
            }

            Content = content;
        }
    }
}
