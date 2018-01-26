using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using QA.DPC.Core.Helpers;
using Quantumart.QPublishing.Database;

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
            QueryFormat = "json";
        }

        [ModelBinder(Name = "customerCode")]
        public string CustomerCode { get; set; }

        [ModelBinder(Name = "instanceId")]
        public string InstanceId { get; set; }

        [BindNever]
        public string FixedConnectionString { get; set; }

        [BindNever]
        public bool UseProductVersions { get; set; }

        public bool IsLive { get; set; }

        [ModelBinder(Name = "language")]
        public string Language { get; set; }

        [ModelBinder(Name = "slug")]
        public string Slug { get; set; }

        public int Version { get; set; }

        [ModelBinder(BinderType = typeof(HeaderFormatModelBinder))]
        public string HeaderFormat { get; set; }

        [ModelBinder(Name = "format")]
        public string QueryFormat { get; set; }

        public string Format => HeaderFormat ?? QueryFormat;

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

        public string GetConnectionString()
        {
            if (!String.IsNullOrEmpty(FixedConnectionString))
                return FixedConnectionString;

            if (string.IsNullOrEmpty(CustomerCode))
                throw new ArgumentNullException(nameof(CustomerCode));

            return DBConnector.GetConnectionString(CustomerCode);

        }

    }
}