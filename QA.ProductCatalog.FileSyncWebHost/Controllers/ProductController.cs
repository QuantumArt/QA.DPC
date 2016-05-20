using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using QA.Core;

namespace QA.ProductCatalog.FileSyncWebHost.Controllers
{
	public class ProductController : ApiController
	{
		private readonly string _path;
		private readonly ILogger _logger;

		public ProductController(ILogger logger)
		{
			_path = ConfigurationManager.AppSettings["Path"];
			_logger = logger;
		}
		
		[HttpPut]
		[HttpDelete]
		[HttpGet]
		public async Task<HttpResponseMessage> Send(string exstension, string format, int productId)
		{
			string fileName = GetFileName(format, exstension, productId);
			string filePath = Path.Combine(_path, fileName);

			if (Request.Method == HttpMethod.Put)
			{
				using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
				{
					await Request.Content.CopyToAsync(fs);
				};
				_logger.LogDebug(() => "add file " + fileName);
			}
			else if (Request.Method == HttpMethod.Delete)
			{
				File.Delete(filePath);
				_logger.LogDebug(() => "delete file " + fileName);
			}

			return Request.CreateResponse(HttpStatusCode.OK, filePath);
		}

		private string GetFileName(string format, string exstension, int productId)
		{

			string name;

			if (format.Contains("{0}"))
			{
				name = string.Format(format, productId);
			}
			else
			{
				name = format + productId;
			}

			return string.Format("{0}.{1}", name, exstension);
		}
	}
}
