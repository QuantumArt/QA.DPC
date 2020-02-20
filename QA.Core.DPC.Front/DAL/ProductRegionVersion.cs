using System.ComponentModel.DataAnnotations.Schema;

namespace QA.Core.DPC.Front.DAL
{
    public class ProductRegionVersion
    {
        public int Id { get; set; }
        
        public int ProductVersionId { get; set; }
        
        public int RegionId { get; set; }
        
        public ProductVersion ProductVersion { get; set; }
    }
}