using System.ComponentModel;
using QA.Core.Models.Tools;

namespace QA.Core.Models.Configuration
{
    public abstract class Association : Field
    {
        [DisplayName("При клонировании родительской сущности")]
        [DefaultValue(CloningMode.Ignore)]
        public CloningMode CloningMode { get; set; }

        [DisplayName("При создании\\обновлении")]
        [DefaultValue(UpdatingMode.Ignore)]
        public UpdatingMode UpdatingMode { get; set; }

        [DisplayName("При удалении родительской сущности или удалении связи")]
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