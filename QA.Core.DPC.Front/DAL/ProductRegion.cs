using System.ComponentModel.DataAnnotations.Schema;

namespace QA.Core.DPC.Front.DAL
{
    public partial class ProductRegion
    {
        public int Id { get; set; }
        
        public int ProductId { get; set; }
        
        public int RegionId { get; set; }
        
        
        public Product Product { get; set; }
    }
}