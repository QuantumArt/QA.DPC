using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using QA.Core.Models.Entities;

namespace QA.Core.DPC.Loader
{
    internal class XmlProductDataSource : IProductDataSource
    {
        private readonly XElement _containerElement;

        public XmlProductDataSource(XElement containerElement)
        {
            if (containerElement == null)
                throw new ArgumentNullException("containerElement");

            _containerElement = containerElement;
        }

        public int? GetInt(string fieldName)
        {
            XElement fieldElement = _containerElement.Element(fieldName);

            return fieldElement == null || fieldElement.Value == string.Empty ? (int?) null : int.Parse(fieldElement.Value);
        }

        public DateTime? GetDateTime(string fieldName)
        {
            XElement fieldElement = _containerElement.Element(fieldName);

            return fieldElement == null || fieldElement.Value == string.Empty ? (DateTime?) null : DateTime.ParseExact(fieldElement.Value, "yyyy-MM-ddTHH:mm:ss", null);
        }

        public decimal? GetDecimal(string fieldName)
        {
            XElement fieldElement = _containerElement.Element(fieldName);

            return fieldElement == null || fieldElement.Value == string.Empty ? (decimal?) null : decimal.Parse(fieldElement.Value, new NumberFormatInfo { NumberDecimalSeparator = "." });
        }

        public IProductDataSource GetContainer(string fieldName)
        {
            XElement fieldElement = _containerElement.Element(fieldName);

            return fieldElement == null ? null : new XmlProductDataSource(fieldElement);
        }

        public string GetString(string fieldName)
        {
            XElement fieldElement = _containerElement.Element(fieldName);

            return fieldElement == null ? null : fieldElement.Value;
        }

        public IEnumerable<IProductDataSource> GetContainersCollection(string fieldName)
        {
            XElement fieldElement = _containerElement.Element(fieldName);

            return fieldElement == null ? null : fieldElement.Elements().Select(x => new XmlProductDataSource(x));
        }
    }
}
