using System;
using System.ComponentModel;

namespace QA.Core.Models.Entities
{
    [Serializable]
    public class BackwardArticleField : MultiArticleField
    {
        [DefaultValue(null)]
        public string RelationGroupName { get; set; }
    }
}