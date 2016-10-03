using System;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace QA.Core.DPC.Integration
{
	[Serializable]
    public class ProductInfo
    {
        [XmlArrayItem(Type = typeof(Product))]    
        public Product[] Products { get; set; }
    }
    [Serializable]
    public class Product 
    {     
        public MarketingProduct MarketingProduct { get; set; }
        public int Id { get; set; }
        [XmlAttribute("productType")]
		[JsonProperty("Type")]
        public string ProductType { get; set; }
        public Region[] Regions { get; set; }

        public string Title { get; set; }

		public string Alias { get; set; }

    }

    [Serializable]
    public class MarketingProduct 
    {       
        public int Id { get; set; }
     
        public string Title { get; set; }

        public string Alias { get; set; }
    }

   

    [Serializable]
    public class Region
    {
      
        public int Id { get; set; }
  
        public string Title { get; set; }
    
        public string Alias { get; set; }
        
    }

    [Serializable]
    public class ProductData
    {

        public string Product { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; } 
    }
}
