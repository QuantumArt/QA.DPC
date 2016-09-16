using System;

namespace QA.Core.Models.Entities
{
    [Serializable]
    public class ExtensionArticleField : SingleArticleField, IGetFieldStringValue
    {
        public string Value { get; set; }
    }
}