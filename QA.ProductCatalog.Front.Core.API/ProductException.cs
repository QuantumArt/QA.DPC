using System;
using QA.Core.DPC.Front;

namespace QA.ProductCatalog.Front.Core.API
{
    public class ProductException : Exception
    {
        public string Result { get; set; }
        
        public int ProductId { get; set; }

        public ProductLocator Locator { get; set; } = new ProductLocator();

        public ProductException()
        {
        }

        public ProductException(string message) : base(message)
        {
            
        }
        
        public ProductException(string message, string result, int productId, ProductLocator locator) : base(message)
        {
            ProductId = productId;
            Result = result;
            Locator = locator;
        }

        public ProductException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

