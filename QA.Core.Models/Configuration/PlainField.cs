using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using QA.Core.Models.Tools;

namespace QA.Core.Models.Configuration
{
    public sealed class PlainField : Field
    {
        [ScaffoldColumn(false)] 
        [DefaultValue(false)]
        public bool ShowInList { get; set; }

        internal override bool RecursiveEquals(Field other, ReferenceDictionary<Content, Content> visitedContents)
        {
            if (!base.RecursiveEquals(other, visitedContents))
            {
                return false;
            }

            return ShowInList == ((PlainField)other).ShowInList;
        }
    }
}