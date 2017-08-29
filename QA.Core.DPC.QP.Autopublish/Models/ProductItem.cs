namespace QA.Core.DPC.QP.Autopublish.Models
{
    public class ProductItem
    {
        public string CustomerCode { get; set; }
        public int ProductId { get; set; }
        public int DefinitionId { get; set; }
        public bool IsUnited { get; set; }
        public string Action { get; set; }
        public bool? IsArchiveOld { get; set; }
        public bool? IsArchiveNew { get; set; }
        public string TypeOld { get; set; }
        public string TypeNew { get; set; }
        public PublishAction PublishAction
        {
            get
            {
                switch (Action)
                {
                    case "updated":
                        if (!IsArchiveNew.Value)
                            return PublishAction.Publish;
                        else if (IsArchiveNew.Value && !IsArchiveOld.Value)
                            return PublishAction.Delete;
                        else
                            return PublishAction.Ignore;
                    case "inserted":
                        if (IsArchiveNew.Value)
                            return PublishAction.Ignore;
                        else
                            return PublishAction.Publish;
                    case "deleted":
                        if (IsArchiveOld.Value)
                            return PublishAction.Ignore;
                        else
                            return PublishAction.Delete;
                    default:
                        return PublishAction.Ignore;
                }
            }
        }
    }

    public enum PublishAction
    {
        Publish,
        Delete,
        Ignore
    }
}
