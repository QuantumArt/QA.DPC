using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using QA.Core;
using QA.Core.DPC.Front;
using QA.Core.Logger;
using QA.Core.Service.Interaction;

namespace QA.ProductCatalog.Front.Core.API.Controllers
{
    [Route("api/{customerCode}/products"), Route("api/products")]
    public class ProductsController : Controller
    {
        protected readonly IDpcProductService ProductService;

        protected readonly ILogger Logger;

        protected readonly IDpcService DpcService;

        protected readonly DataOptions Options;

        public ProductsController(IDpcProductService productService, ILogger logger, IDpcService dpcService, IOptions<DataOptions> options)
        {
            ProductService = productService;
            Logger = logger;
            DpcService = dpcService;
            Options = options.Value;
        }

        [HttpGet]
        [HttpGet("{language}/{state}")]
        public ActionResult GetProductIds(ProductLocator locator, int page, DateTime? date, int pageSize = Int32.MaxValue)
        {
            ApplyOptions(locator);
            var ints = (date == null)
                ? DpcService.GetAllProductId(locator, page, pageSize)
                : DpcService.GetLastProductId(locator, page, pageSize, date.Value);

            if (locator.Format == "json")
            {
                return Json(ints);
            }
            else
            {
                var ints2 = String.Join("", ints.Select(n => $"<id>{n}</id>").ToList());
                return Content($"<ids>{ints2}</ids>", XmlHeader);
            }

        }              

        [HttpGet("{id:int}")]
        [HttpGet("{language}/{state}/{id:int}")]
        public ActionResult GetProduct(ProductLocator locator, int id, DateTime? date)
        {
            ApplyOptions(locator);
            var data = DpcService.GetProductData(locator, id);

            if (data == null)
                return BadRequest($"Product {id} is not found");

            ControllerContext.HttpContext.Response.Headers.Add("Last-Modified", data.Updated.ToUniversalTime().ToString("R"));
            return (locator.Format == "json")
                ? Content(data.Product, JsonHeader)
                : Content(data.Product, XmlHeader);

        }

        [HttpGet]
        [HttpGet("{language}/{state}/{date}")]
        public ActionResult GetProductVersionIds(ProductLocator locator, int page, DateTime date, int pageSize = Int32.MaxValue)
        {
            ApplyOptions(locator);
            var ints = DpcService.GetAllProductVersionId(locator, page, pageSize, date);

            if (locator.Format == "json")
            {
                return Json(ints);
            }
            else
            {
                var ints2 = String.Join("", ints.Select(n => $"<id>{n}</id>").ToList());
                return Content($"<ids>{ints2}</ids>", XmlHeader);
            }
        }


        [HttpGet("{id:int}")]
        [HttpGet("{language}/{state}/{date}/{id:int}")]
        public ActionResult GetProductVersion(ProductLocator locator, int id, DateTime date)
        {
            ApplyOptions(locator);
            var data = DpcService.GetProductVersionData(locator, id, date);

            if (data == null)
                return BadRequest($"Product version {id} is not found");

            return (locator.Format == "json")
                ? Content(data.Product, JsonHeader)
                : Content(data.Product, XmlHeader);
        }

        private static MediaTypeHeaderValue JsonHeader => new MediaTypeHeaderValue("application/json") { Charset = Encoding.UTF8.WebName};

        private static MediaTypeHeaderValue XmlHeader => new MediaTypeHeaderValue("application/xml") { Charset = Encoding.UTF8.WebName};

        [HttpDelete]
        [HttpDelete("{language}/{state}")]
        public ActionResult DeleteProduct(ProductLocator locator, [FromBody] string data)
        {
            ApplyOptions(locator);
            var res1 = ProductService.Parse(locator, data);
            if (res1.IsSucceeded && res1.Result?.Products != null)
            {
                Logger.Info("Message parsed, deleting products... ");

                try
                {
                    foreach (var p in res1.Result.Products)
                    {
                        Logger.Info($"Deleting product {p.Id}...");

                        var res2 = ProductService.DeleteProduct(locator, p.Id, data);
                        if (!res2.IsSucceeded)
                        {
                            throw new Exception($"Error while deleting product {p.Id}: {res2.Error.Message}");
                        }

                        Logger.Info($"Product {p.Id} successfully deleted");
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                    return BadRequest(e.Message);
                }

                return Ok();
            }
            else
            {
                return ProceedParseError(data, res1);
            }

        }

        [HttpPut]
        [HttpPut("{language}/{state}")]
        public ActionResult PutProduct(ProductLocator locator, [FromBody] string data, [FromQuery(Name = "UserId")] int userId, [FromQuery(Name = "UserName")] string userName)
        {
            ApplyOptions(locator);
            var res1 = ProductService.Parse(locator, data);
            if (res1.IsSucceeded && res1.Result?.Products?.Any() == true)
            {
                Logger.Info("Message parsed, creating or updating products...  ");

                try
                {
                    foreach (var p in res1.Result.Products)
                    {
                        Logger.Info($"Check for creating or updating (product {p.Id})");
                        var res2 = ProductService.HasProductChanged(locator, p.Id, data);
                        if (!res2.IsSucceeded)
                        {
                            throw new Exception($"Error while checking product {p.Id}: {res2.Error.Message}");
                        }
                        else if (!res2.Result)
                        {
                            Logger.Info($"Product {p.Id} doesn't require updating");
                        }
                        else
                        {
                            Logger.Info($"Creating or updating product {p.Id}");

                            var res3 = ProductService.UpdateProduct(locator, p, data, userName, userId);
                            if (res3.IsSucceeded)
                            {
                                Logger.Info($"Product {p.Id} successfully created/updated");
                            }
                            else
                            {
                                throw new Exception($"Error while creating/updating product {p.Id}: {res3.Error.Message}");
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                    return BadRequest(e.Message);
                }
                return Ok();
            }
            else
            {
                return ProceedParseError(data, res1);
            }
        }

        private ActionResult ProceedParseError(string data, ServiceResult<ProductInfo> res1)
        {
            string result;
            if (!res1.IsSucceeded)
            {
                result = res1.Error.Message;
                Logger.Error($"Could not parse message to products. Message: {result}. Data: {data}");
            }
            else
            {
                result = "Could find products in parsed message";
                Logger.Error(result);
            }
            return BadRequest(result);
        }

        private void ApplyOptions(ProductLocator locator)
        {
            if (!String.IsNullOrEmpty(Options.FixedConnectionString))
            {
                locator.FixedConnectionString = Options.FixedConnectionString;                
            }

            locator.UseProductVersions = Options.UseProductVersions;
        }
    }
}
