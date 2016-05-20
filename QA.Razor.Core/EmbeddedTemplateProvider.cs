// Owners: Karlov Nikolay, Abretov Alexey

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QA.Razor.Core
{
    public class EmbeddedTemplateProvider : EmbeddedViewProvider
    {
        public override string[] ViewPath
        {
            get
            {
                return new string[] 
                {
                    "{0}.Views.{1}",
                    "{0}.Templates.{1}",
                    "{0}.{1}"
                };
            }
        }
    }
}
