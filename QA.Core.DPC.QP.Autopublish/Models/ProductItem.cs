namespace QA.Core.DPC.QP.Autopublish.Models
{
    public class ProductItem
    {
        public string CustomerCode { get; set; }
        public int ProductId { get; set; }
        public int DefinitionId { get; set; }
        public bool IsUnited { get; set; }
        public string Action { get; set; }
    }
}
