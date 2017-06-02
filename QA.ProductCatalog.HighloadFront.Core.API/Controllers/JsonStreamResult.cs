using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace QA.ProductCatalog.HighloadFront.Core.API.Controllers
{
    public class JsonStreamResult : ActionResult
    {
        public JsonStreamResult(Stream stream)
        {
            Stream = stream;
        }

        public Stream Stream { get; set; }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            var media = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" }.ToString();
            context.HttpContext.Response.ContentType = media;
            await Stream.CopyToAsync(context.HttpContext.Response.Body);

        }
    }
}