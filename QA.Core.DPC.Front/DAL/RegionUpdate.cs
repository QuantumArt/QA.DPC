using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace QA.Core.DPC.Front.DAL
{
    public class RegionUpdate
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]        
        public int Id { get; set; }
        
        public DateTime Updated { get; set; }
        
        public int RegionId { get; set; }
    }
}