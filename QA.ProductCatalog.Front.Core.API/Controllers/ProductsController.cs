using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using QA.Core;
using QA.Core.DPC.Front;

namespace QA.ProductCatalog.Front.Core.API.Controllers
{
    [Route("api/[controller]")]
    public class ProductsController : Controller
    {
        protected readonly IDpcProductService ProductService;

        protected readonly ILogger Logger;

        protected readonly IDpcService DpcService;

        public ProductsController(IDpcProductService productService, ILogger logger, IDpcService dpcService)
        {
            ProductService = productService;
            Logger = logger;
            DpcService = dpcService;
        }

        [HttpGet]
        [HttpGet("{language}/{state}")]

        public ActionResult GetProductIds(ProductLocator locator, int page, DateTime? date, int pageSize = Int32.MaxValue)
        {
            var ints = (date == null)
                ? DpcService.GetAllProductId(locator, page, pageSize)
                : DpcService.GetLastProductId(locator, page, pageSize, date.Value);
            return Json(ints);
        }

        [HttpGet("{id:int}")]
        [HttpGet("{language}/{state}/{id:int}")]
        public ActionResult GetProduct(ProductLocator locator, int id, DateTime? date)
        {
            var data = DpcService.GetProductData(locator, id);
            ControllerContext.HttpContext.Response.Headers.Add("Last-Modified", data.Updated.ToUniversalTime().ToString("R"));
            return Content(data.Product, new MediaTypeHeaderValue("application/json") { Charset = Encoding.UTF8.WebName });
        }

        [HttpDelete]
        [HttpDelete("{language}/{state}")]
        public ActionResult DeleteProduct(ProductLocator locator, [FromBody] string data)
        {
            var res1 = ProductService.Parse(locator, data);
            if (res1.IsSucceeded && res1.Result?.Products != null)
            {
                Logger.Info("��������� ���������, ������� �������� ... ");

                try
                {
                    foreach (var p in res1.Result.Products)
                    {
                        Logger.Info($"������� ������� {p.Id}");

                        var res2 = ProductService.DeleteProduct(locator, p.Id);
                        if (!res2.IsSucceeded)
                        {
                            throw new Exception($"������ ��� �������� �������� {p.Id}: {res2.Error.Message}");
                        }

                        Logger.Info($"������� {p.Id} ������� ������");
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
                Logger.Info($"�� ������� ��������� ��������� � ��������: {data}");
                if (!res1.IsSucceeded)
                    return BadRequest(res1.Error.Message);
                else
                    return BadRequest("�� ������� ��������� ��������� � ��������");
            }

        }

        [HttpPut]
        [HttpPut("{language}/{state}")]
        public ActionResult PutProduct(ProductLocator locator, [FromBody] string data, [FromQuery(Name = "UserId")] int userId, [FromQuery(Name = "UserName")] string userName)
        {
            var res1 = ProductService.Parse(locator, data);
            if (res1.IsSucceeded && res1.Result?.Products?.Any() == true)
            {
                Logger.Info("��������� ���������, �������/��������� ��������... ");

                try
                {
                    foreach (var p in res1.Result.Products)
                    {
                        Logger.Info($"���������, ����� �� ���������/��������� ������� {p.Id}");
                        var res2 = ProductService.HasProductChanged(locator, p.Id, data);
                        if (!res2.IsSucceeded)
                        {
                            Logger.Info($"������ ��� �������� �������� {p.Id}: {res2.Error.Message}");
                            throw new Exception(res2.Error.Message);
                        }
                        else if (!res2.Result)
                        {
                            Logger.Info($"������� {p.Id} �� ������� ��������/����������");
                        }
                        else
                        {
                            Logger.Info($"�������/��������� ������� {p.Id}");

                            var res3 = ProductService.UpdateProduct(locator, p, data, userName, userId);
                            if (res3.IsSucceeded)
                            {
                                Logger.Info($"������� {p.Id} ������� ������/��������");
                            }
                            else
                            {
                                Logger.Info($"������ ��� ��������/���������� �������� {p.Id}: {res3.Error.Message}");
                                throw new Exception(res3.Error.Message);
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
                return Ok();
            }
            else
            {
                Logger.Info($"�� ������� ��������� ��������� � ��������: {data}");
                if (!res1.IsSucceeded)
                    return BadRequest(res1.Error.Message);
                else
                    return BadRequest("�� ������� ��������� ��������� � ��������");

            }
        }
    }
}
