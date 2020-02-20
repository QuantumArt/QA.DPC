#if !NETSTANDARD
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace QA.Core.DPC.Formatters.Formatting
{
	public class RouteDataMapping : MediaTypeMapping
	{
		private readonly string _routeDataValueName;
		private readonly string _routeDataValueValue;

		public RouteDataMapping(
			string routeDataValueName,
			string routeDataValueValue,
			MediaTypeHeaderValue mediaType)
			: base(mediaType)
		{
			_routeDataValueName = routeDataValueName;
			_routeDataValueValue = routeDataValueValue;
		}

		public RouteDataMapping(
			string routeDataValueName,
			string routeDataValueValue,
			string mediaType)
			: base(mediaType)
		{

			_routeDataValueName = routeDataValueName;
			_routeDataValueValue = routeDataValueValue;
		}

		public override double TryMatchMediaType(HttpRequestMessage request)
		{
			object value;
			request.GetRouteData().Values.TryGetValue(_routeDataValueName, out value);
			
			if (value == null)
			{
				return 0;
			}
			else
			{
				return value.ToString() == _routeDataValueValue ? 1 : 0;
			}
		}
	}
}
#endif