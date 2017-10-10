namespace QA.Core.DPC.QP.Autopublish.Models
{
    public class ProductItem
    {
        private const string PublishedStatus = "Published";
        private const string UpdatedAction = "updated";
        private const string InsertedAction = "inserted";
        private const string DeletedAction = "deleted";

        public string CustomerCode { get; set; }
        public int ProductId { get; set; }
        public int DefinitionId { get; set; }
        public bool IsUnited { get; set; }
        public string Action { get; set; }
        public bool? IsArchiveOld { get; set; }
        public bool? IsArchiveNew { get; set; }
        public bool? IsVisibleOld { get; set; }
        public bool? IsVisibleNew { get; set; }
        public string TypeOld { get; set; }
        public string TypeNew { get; set; }
        public string StatusOld { get; set; }
        public string StatusNew { get; set; }
        public PublishAction PublishAction
        {
            get
            {
                var status = Action == DeletedAction ? StatusOld : StatusNew;
                var notPublishedLive = !IsUnited && status != PublishedStatus;

                if (notPublishedLive)
                {
                    return PublishAction.Ignore;
                }

                switch (Action)
                {
                    case UpdatedAction:
                        if (!IsHiddenNew)
                            return PublishAction.Publish;
                        else if (IsHiddenNew && !IsHiddenOld)
                            return PublishAction.Delete;
                        else
                            return PublishAction.Ignore;
                    case InsertedAction:
                        if (IsHiddenNew)
                            return PublishAction.Ignore;
                        else
                            return PublishAction.Publish;
                    case DeletedAction:
                        if (IsHiddenOld)
                            return PublishAction.Ignore;
                        else
                            return PublishAction.Delete;
                    default:
                        return PublishAction.Ignore;
                }
            }
        }
        
        private bool IsHiddenNew
        {
            get { return IsArchiveNew.Value || !IsVisibleNew.Value; }
        }

        private bool IsHiddenOld
        {
            get { return IsArchiveOld.Value || !IsVisibleOld.Value; }
        }
    }

    public enum PublishAction
    {
        Publish,
        Delete,
        Ignore
    }
}
