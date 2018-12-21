using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.API
{
	public class ProductAPIProxy : IProductAPIService
	{
		private readonly IProxyConfiguration _configuration;
		private readonly Func<Type, MediaTypeFormatter> _getFormatter;

		public ProductAPIProxy(IProxyConfiguration configuration, Func<Type, MediaTypeFormatter> getFormatter)
		{
			_configuration = configuration;
			_getFormatter = getFormatter;
		}

		#region IProductAPIService implementation
		public Dictionary<string, object>[] GetProductsList(string slug, string version, bool isLive = false, long startRow = 0, long pageSize = int.MaxValue)
		{
			string url = _configuration.Host + "/" + version + "/" + slug + "/binary?isLive=" + isLive + "&startRow=" + startRow + "&pageSize=" + pageSize;
			var result = Get<Dictionary<string, object>[]>(url);
			return result;
		}

		public int[] SearchProducts(string slug, string version, string query, bool isLive = false)
		{
			string url = _configuration.Host + "/" + version + "/" + slug + "/search/binary/" + query + "?isLive=" + isLive;
			var result = Get<int[]>(url);
			return result;
		}

        public int[] ExtendedSearchProducts(string slug, string version, JToken query, bool isLive = false)
        {
            string url = _configuration.Host + "/" + version + "/" + slug + "/search/extended/binary/" + query + "?isLive=" + isLive;
            var result = Get<int[]>(url);
            return result;
        }

        public Article GetProduct(string slug, string version, int id, bool isLive = false)
		{
			string url = _configuration.Host + "/" + version + "/" + slug + "/binary/" + id + "?isLive=" + isLive;
			var result = Get<Article>(url);
			return result;
		}

		public void UpdateProduct(string slug, string version, Article product, bool isLive = false)
		{
			string url = _configuration.Host + "/" + version + "/" + slug + "/binary/" + product.Id + "?isLive=" + isLive;
			Update<Article>(product, url);			
		}


		public void CustomAction(string actionName, int id, int contentId = default(int))
		{
			string url = _configuration.Host + "/custom/binary/" + actionName + "/" + id;
			Post(url);
		}

		public void CustomAction(string actionName, int[] ids, int contentId = default(int))
		{
			throw new NotImplementedException();
		}

		public void CustomAction(string actionName, int id, Dictionary<string, string> parameters, int contentId = default(int))
		{
			string url = _configuration.Host + "/custom/binary/" + actionName + "/" + id;
			Update<Dictionary<string, string>>(parameters, url);
		}

		public void CustomAction(string actionName, int[] ids, Dictionary<string, string> parameters, int contentId = default(int))
		{
			throw new NotImplementedException();
		}

		public ServiceDefinition GetProductDefinition(string slug, string version, bool forList = false)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Private methods
		public async Task UpdateAsync<T>(T model, string url)
		{
			try
			{
				var formatter = _getFormatter(typeof(T));

				using (var client = new HttpClient())
				using (var response = await client.PostAsync<T>(url, model, formatter))
				{
					await ValidateResponseAsync(response);
				}
			}
			catch (ProxyExternalException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new ProxyException("post", ex);
			}
		}

		public void Update<T>(T model, string url)
		{
			try
			{
				var formatter = _getFormatter(typeof(T));

				using (var client = new HttpClient())
				using (var response = client.PostAsync<T>(url, model, formatter).Result)
				{
					ValidateResponse(response);
				}
			}
			catch (ProxyExternalException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new ProxyException("post", ex);
			}
		}

		public async Task<T> GetAsync<T>(string url)
		{
			try
			{
				using (var client = new HttpClient())
				using (var response = await client.GetAsync(url))
				{
					await ValidateResponseAsync(response);
					var formatter = _getFormatter(typeof(T));
					return await response.Content.ReadAsAsync<T>(new[] { formatter });
				}
			}
			catch (ProxyExternalException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new ProxyException("get", ex);
			}
		}
		
		public T Get<T>(string url)
		{
			try
			{
				using (var client = new HttpClient())
				using (var response = client.GetAsync(url).Result)
				{
					ValidateResponse(response);
					var formatter = _getFormatter(typeof(T));
					return response.Content.ReadAsAsync<T>(new[] { formatter }).Result;
				}
			}
			catch (ProxyExternalException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new ProxyException("get", ex);
			}
		}

		public async Task DeleteAsync(string url)
		{
			try
			{
				using (var client = new HttpClient())
				using (var response = await client.DeleteAsync(url))
				{
					await ValidateResponseAsync(response);
				}
			}
			catch (ProxyExternalException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new ProxyException("delete", ex);
			}
		}

		public void Delete(string url)
		{
			try
			{
				using (var client = new HttpClient())
				using (var response = client.DeleteAsync(url).Result)
				{
					ValidateResponse(response);
				}
			}
			catch (ProxyExternalException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new ProxyException("delete", ex);
			}
		}

		public void Post(string url)
		{
			try
			{
				using (var client = new HttpClient())
				using (var content = new ByteArrayContent(new byte[0]))
				using (var response = client.PostAsync(url, content).Result)
				{
					ValidateResponse(response);
				}
			}
			catch (ProxyExternalException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new ProxyException("post", ex);
			}
		}

		private async Task ValidateResponseAsync(HttpResponseMessage response)
		{
			if (!response.IsSuccessStatusCode)
			{
				var exceptionFormatter = _getFormatter(typeof(Exception));
				var ex = await response.Content.ReadAsAsync<Exception>(new[] { exceptionFormatter });
				throw new ProxyExternalException("invalid response : " + response.StatusCode, ex);
			}
		}

		private void ValidateResponse(HttpResponseMessage response)
		{
			if (!response.IsSuccessStatusCode)
			{
				Exception exception = null;

				try
				{
					var exceptionFormatter = _getFormatter(typeof(Exception));
					exception = response.Content.ReadAsAsync<Exception>(new[] { exceptionFormatter }).Result;
				}
				catch (Exception ex)
				{
					exception = ex;
				}

				throw new ProxyExternalException(response.ReasonPhrase, exception);
			}
		}

		private void CaptureStackTraceAndThrow(Exception ex)
		{
			var capture = Delegate.CreateDelegate(typeof(ThreadStart), ex, "InternalPreserveStackTrace", false, false) as ThreadStart;
			
			if (capture != null)
			{
				capture();
			}

			throw ex;
		}
        #endregion
    }
}
