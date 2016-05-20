using System;
using System.Collections.Generic;
using System.Windows.Markup;

namespace QA.ProductCatalog.HighloadFront.Infrastructure.Configuration
{
    [ContentProperty("Clients")]
    public class AccessControlList
    {
        public AccessControlList()
        {
            Clients = new List<Client>();
        }

        // тестовые поля для демо
        public int IntProperty { get; set; }
        public TimeSpan Interval { get; set; }
        public IList<Client> Clients { get; set; }
    }

    public class Client
    {
        public string Name { get; set; }
        public string AccessToken { get; set; }
        public int Limit { get; set; }
        public Quote Quotas { get; set; }
    }

    public class Quote
    {
        public int Limit1 { get; set; }
        public int Limit2 { get; set; }

        public TimeSpan Period { get; set; }
        public bool Enabled { get; set; }
        public string Comment { get; set; }
    }
}
