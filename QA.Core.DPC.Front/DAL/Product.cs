using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace QA.Core.DPC.Front.DAL
{
    public partial class Product
    {

        public Product()
        {
            ProductRegions = new HashSet<ProductRegion>();
        }
        
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public int DpcId { get; set; }
        
        public string Slug { get; set; }
        
        public int Version { get; set; }
        
        public bool IsLive { get; set; }
        
        public string Language { get; set; }
        
        public string Format { get; set; }
        
        public string Data { get; set; }
        
        public string Alias { get; set; }
        
        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }
        
        public string Hash { get; set; }
        
        public int? MarketingProductId { get; set; }
        
        public string Title { get; set; }
        
        public string UserUpdated { get; set; }
        
        public int? UserUpdatedId { get; set; }
        
        public string ProductType { get; set; }
        
        public ICollection<ProductRegion> ProductRegions { get; set; }
        
    }
}