using System;

namespace QA.Core.DPC.Front
{
    public class ProductLocator
    {
        public ProductLocator()
        {
            IsLive = true;
            Language = "invariant";
            Version = 1;
            Slug = string.Empty;
            QueryFormat = "json";
        }
        
        public string CustomerCode { get; set; }

        public string InstanceId { get; set; }

        public bool UseProductVersions { get; set; }

        public bool IsLive { get; set; }

        public string Language { get; set; }

        public string Slug { get; set; }

        public int Version { get; set; }

        public string HeaderFormat { get; set; }

        public string QueryFormat { get; set; }

        public string Format => HeaderFormat ?? QueryFormat;

        public string State
        {
            get => (IsLive) ? "live" : "stage";
            set => IsLive = value.Equals("live", StringComparison.InvariantCulture);
        }
    }
}