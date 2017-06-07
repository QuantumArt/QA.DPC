using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using QA.DPC.Core.Helpers;

namespace QA.Core.DPC.Front
{
    public class ProductLocator
    {
        public ProductLocator()
        {
            IsLive = true;
            Language = "invariant";
            Version = 1;
            Slug = String.Empty;
        }

        [ModelBinder(Name = "customerCode")]
        public string CustomerCode { get; set; }

        public bool IsLive { get; set; }

        [ModelBinder(Name = "language")]
        public string Language { get; set; }

        [ModelBinder(Name = "slug")]
        public string Slug { get; set; }

        public int Version { get; set; }

        [ModelBinder(BinderType = typeof(FormatModelBinder))]
        public string Format { get; set; }

        [ModelBinder(Name = "state")]
        public string State
        {
            get => (IsLive) ? "live" : "stage";
            set => IsLive = value.Equals("live", StringComparison.InvariantCulture);
        }

        [ModelBinder(Name = "version")]
        public string StringVersion
        {
            get => $"v{Version}.0";
            set => Version = int.Parse(Regex.Match(value, @"^v([\d]+).[\d]+").Value);
        }

        public IProductSerializer GetSerialiser()
        {
            if (Format == "json")
            {
                return new JsonProductSerializer();
            }
            return new XmlProductSerializer();
        }
    }
}