using System.IO;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QA.Core.Logger;

namespace QA.ProductCatalog.FileSyncWebHost.Controllers
{
	[Route("Product/Send")]
    public class ProductController : Controller
	{
		private readonly DataOptions _opts;
		private readonly ILogger _logger;

		public ProductController(ILogger logger, IOptions<DataOptions> opts)
		{
			_opts = opts.Value;
			_logger = logger;
		}
		
		[HttpPut("{format}/{extension}")]
		public ActionResult PutResult([FromBody] string data, string extension, string format, int productId)
		{
			var fileName = GetFileName(format, extension, productId);
			var filePath = Path.Combine(_opts.FilePath, fileName);
			if (!Directory.Exists(_opts.FilePath))
			{
				Directory.CreateDirectory(_opts.FilePath);
			}
			using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
			{
				var array = Encoding.UTF8.GetBytes(data);
				fs.Write(array);
			};
			_logger.LogDebug(() => "add file " + fileName);
			return Content(filePath);
		}

		[HttpDelete("{format}/{extension}")]
		public ActionResult DeleteResult(string extension, string format, int productId)
		{
			var fileName = GetFileName(format, extension, productId);
			var filePath = Path.Combine(_opts.FilePath, fileName);
			if (System.IO.File.Exists(filePath))
			{
				System.IO.File.Delete(filePath);
				_logger.LogDebug(() => "delete file " + fileName);
				return Content(filePath);
			}

			return NotFound();
		}
		
		[HttpGet("{format}/{extension}")]
		public ActionResult GetResult(string extension, string format, int productId)
		{
			var fileName = GetFileName(format, extension, productId);
			var filePath = Path.Combine(_opts.FilePath, fileName);
			if (System.IO.File.Exists(filePath))
			{
				return Content(filePath);
			}
			return NotFound();
		}
		

		private string GetFileName(string format, string extension, int productId)
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

			return $"{name}.{extension}";
		}
	}
}
