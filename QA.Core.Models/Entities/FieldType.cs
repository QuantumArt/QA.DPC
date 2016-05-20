using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QA.Core.Models.Entities
{
    /// <summary>
    /// Типы полей
    /// </summary>
    public enum FieldType
    {
        Unknown,
        
        /// <summary>
        /// текст, число, дата, признак
        /// </summary>
        Plain,

        /// <summary>
        /// 
        /// </summary>
        Entity,
        Aggreragion,
        Association,
        Collection
    }
}
