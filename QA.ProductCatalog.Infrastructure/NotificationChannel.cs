
using System.Globalization;
using System.Xml.XPath;
namespace QA.ProductCatalog.Infrastructure
{
	public class NotificationChannel
	{
		public NotificationChannel()
		{
			DegreeOfParallelism = int.MaxValue;
		}

		public string Name { get; set; }

		public string Url { get; set; }

		/// <summary>
		/// количество одновременно обрабатываемых сообщений
		/// </summary>
		public int DegreeOfParallelism { get; set; }

		/// <summary>
		/// stage или live
		/// </summary>
		public bool IsStage { get; set; }

		/// <summary>
		/// опциональный фильтр по продукту
		/// </summary>
		public string Filter { get; set; }

		private string _xPathFilter = null;

		/// <summary>
		/// опциональный фильтр по xml продукта
		/// </summary>
		public string XPathFilter
		{
			get => _xPathFilter;
		    set
			{
				_xPathFilter = value;

				XPathFilterExpression = string.IsNullOrWhiteSpace(_xPathFilter) ? null : XPathExpression.Compile(_xPathFilter);
			}
		}

		public XPathExpression XPathFilterExpression { get; private set; }

		public string Format { get; set; }
		public string Formatter { get; set; }
		public string MediaType { get; set; }

        public string Language { get; set; }

        public CultureInfo Culture => string.IsNullOrEmpty(Language) ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(Language);

	    public bool IsEqualTo(NotificationChannel channel)
	    {
	        if (channel == null)
            {
                return false;
            }
	        return
	            DegreeOfParallelism == channel.DegreeOfParallelism &&
	            Filter == channel.Filter &&
	            Format == channel.Format &&
	            Formatter == channel.Formatter &&
	            IsStage == channel.IsStage &&
	            MediaType == channel.MediaType &&
	            Name == channel.Name &&
	            Url == channel.Url &&
	            Language == channel.Language;
	    }
    }
}
