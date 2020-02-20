using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using QA.Core.DPC.Resources;
using QA.Core.Models.Tools;

namespace QA.Core.Models.Configuration
{
    public abstract class Association : Field
    {
        [Display(Name="CloningMode", ResourceType = typeof(ControlStrings))]
        [DefaultValue(CloningMode.Ignore)]
        public CloningMode CloningMode { get; set; }

        [Display(Name="UpdatingMode", ResourceType = typeof(ControlStrings))]
        [DefaultValue(UpdatingMode.Ignore)]
        public UpdatingMode UpdatingMode { get; set; }

        [Display(Name="DeletingMode", ResourceType = typeof(ControlStrings))]
        [DefaultValue(DeletingMode.Keep)]
        public DeletingMode DeletingMode { get; set; }

        public abstract Content[] GetContents();

        protected Association()
        {
            CloningMode = CloningMode.Ignore;
            UpdatingMode = UpdatingMode.Ignore;
            DeletingMode = DeletingMode.Keep;
        }

        internal override bool RecursiveEquals(Field other, ReferenceDictionary<Content, Content> visitedContents)
        {
            if (!base.RecursiveEquals(other, visitedContents))
            {
                return false;
            }

            var otherAsAssociation = (Association)other;

            return CloningMode == otherAsAssociation.CloningMode
                   && DeletingMode == otherAsAssociation.DeletingMode
                   && UpdatingMode == otherAsAssociation.UpdatingMode;
        }

        public override int GetHashCode()
        {
            int hash = base.GetHashCode();

            hash = HashHelper.CombineHashCodes(hash, CloningMode.GetHashCode());
            hash = HashHelper.CombineHashCodes(hash, DeletingMode.GetHashCode());
            hash = HashHelper.CombineHashCodes(hash, UpdatingMode.GetHashCode());

            return hash;
        }
    }
}