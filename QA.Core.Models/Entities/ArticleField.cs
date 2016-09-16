using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace QA.Core.Models.Entities
{
    [Serializable]
    [DebuggerDisplay("{FieldId} {FieldName}")]
    public abstract class ArticleField : IModelObject
    {
        [DefaultValue(null)]
        public IReadOnlyDictionary<string, object> CustomProperties { get; set; }

        public int? ContentId { get; set; }

        public int? FieldId { get; set; }

        public string FieldName { get; set; }

        public string FieldDisplayName { get; set; }
    }
}
