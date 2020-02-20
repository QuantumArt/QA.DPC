using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using QA.Core.DPC.Front.DAL;

namespace QA.Core.DPC.Front
{
    public partial class ProductVersion
    {
        public ProductVersion()
        {
            ProductRegionVersions = new HashSet<ProductRegionVersion>();
        }
        
        public int Id { get; set; }
        
        public bool Deleted { get; set; }
        
        public DateTime Modification { get; set; }
        
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
        
        public ICollection<ProductRegionVersion> ProductRegionVersions { get; set; }        
    }
}