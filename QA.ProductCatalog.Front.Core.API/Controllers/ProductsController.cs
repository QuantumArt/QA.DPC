using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using QA.Core.DPC.Front;
using QA.Core.DPC.QP.Services;
using QA.Core.Service.Interaction;
using QA.ProductCatalog.Front.Core.API.ActionResults;
using Quantumart.QPublishing.Database;
using NLog;
using NLog.Fluent;
    

namespace QA.ProductCatalog.Front.Core.API.Controllers
{
    [Route("api")]
    public class ProductsController : Controller
    {
        protected readonly IDpcProductService ProductService;

        protected static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        protected readonly IDpcService DpcService;

        protected readonly DataOptions Options;
        
        public ProductsController(IDpcProductService productService, IDpcService dpcService, DataOptions options)
        {
            ProductService = productService;
            DpcService = dpcService;
            Options = options;
        }

        [HttpGet("products", Name = "List")]
        [HttpGet("products/{language}/{state}", Name = "List-Channel")]
        [HttpGet("{customerCode}/products", Name = "List-Consolidate")]
        [HttpGet("{customerCode}/products/{language}/{state}", Name = "List-Consolidate-Channel")]
        public ActionResult GetProductIds(
            DateTime? filterDate = null, 
            string format = "json", 
            string instanceId = null, 
            int page = 0, 
            int pageSize = Int32.MaxValue
        )
        {
            var locator = new ProductLocator(){ QueryFormat = format, InstanceId = instanceId };         
            ApplyOptions(locator);
            
            var ints = (filterDate == null)
                ? DpcService.GetAllProductId(locator, page, pageSize)
                : DpcService.GetLastProductId(locator, page, pageSize, filterDate.Value);

            if (locator.Format == "json")
            {
                return Json(ints);
            }

            var ints2 = String.Join("", ints.Select(n => $"<id>{n}</id>").ToList());
            return Content($"<ids>{ints2}</ids>", XmlHeader);

        }              

        [HttpGet("products/{id:int}", Name = "Id")]
        [HttpGet("products/{language}/{state}/{id:int}", Name = "Id-Channel")]
        [HttpGet("{customerCode}/products/{id:int}", Name = "Id-Consolidate")]
        [HttpGet("{customerCode}/products/{language}/{state}/{id:int}", Name = "Id-Consolidate-Channel")]        
        public ActionResult GetProduct(int id, string format = "json", string instanceId = null)
        {
            var locator = new ProductLocator(){ QueryFormat = format, InstanceId = instanceId };         
            ApplyOptions(locator);
            
            var data = DpcService.GetProductData(locator, id);

            if (data == null)
            {
                _logger.Error()
                    .Message("Product {productId} is not found", id)
                    .Property("locator", locator)
                    .Write();
                
                return BadRequest($"Product {id} is not found");
                
            }

            ControllerContext.HttpContext.Response.Headers.Add("Last-Modified", data.Updated.ToUniversalTime().ToString("R"));
            return (locator.Format == "json")
                ? Content(data.Product, JsonHeader)
                : Content(data.Product, XmlHeader);

        }

        [HttpGet("products/{filterDate:datetime}", Name = "Versions")]
        [HttpGet("products/{language}/{state}/{filterDate:datetime}", Name = "Versions-Channel")]
        [HttpGet("{customerCode}/products/{filterDate:datetime}", Name = "Versions-Consolidate")]
        [HttpGet("{customerCode}/products/{language}/{state}/{filterDate:datetime}", Name = "Versions-Consolidate-Channel")]
        public ActionResult GetProductVersionIds(DateTime filterDate, string format = "json", 
            string instanceId = null, int page = 0, int pageSize = Int32.MaxValue)
        {
            var locator = new ProductLocator(){QueryFormat = format, InstanceId = instanceId };         
            ApplyOptions(locator);
            
            var ints = DpcService.GetAllProductVersionId(locator, page, pageSize, filterDate);

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


        [HttpGet("products/{filterDate:datetime}/{id:int}", Name = "Versions-Id")]
        [HttpGet("products/{language}/{state}/{filterDate:datetime}/{id:int}", Name = "Versions-Id-Channel")]
        [HttpGet("{customerCode}/products/{filterDate:datetime}/{id:int}", Name = "Versions-Id-Consolidate")]
        [HttpGet("{customerCode}/products/{language}/{state}/{filterDate:datetime}/{id:int}", Name = "Versions-Id-Consolidate-Channel")]        
        public ActionResult GetProductVersion(int id, DateTime filterDate, string format = "json", string instanceId = null)
        {
            var locator = new ProductLocator(){QueryFormat = format, InstanceId = instanceId };         
            ApplyOptions(locator);            
            var data = DpcService.GetProductVersionData(locator, id, filterDate);

            if (data == null)
            {
                _logger.Error()
                    .Message("Product version {versionId} is not found", id)
                    .Property("locator", locator)
                    .Write();       
                
                return BadRequest($"Product version {id} is not found");
            }


            return (locator.Format == "json")
                ? Content(data.Product, JsonHeader)
                : Content(data.Product, XmlHeader);
        }

        private static MediaTypeHeaderValue JsonHeader => new MediaTypeHeaderValue("application/json") { Charset = Encoding.UTF8.WebName};

        private static MediaTypeHeaderValue XmlHeader => new MediaTypeHeaderValue("application/xml") { Charset = Encoding.UTF8.WebName};

        [HttpDelete("products", Name = "Delete")]
        [HttpDelete("products/{language}/{state}", Name = "Delete-Channel")]
        [HttpDelete("{customerCode}/products", Name = "Delete-Consolidate")]
        [HttpDelete("{customerCode}/products/{language}/{state}", Name = "Delete-Consolidate-Channel")]        
        public ActionResult DeleteProduct([FromBody] string data, string format = "json", string instanceId = null)
        {
            var locator = new ProductLocator(){QueryFormat = format, InstanceId = instanceId };         
            ApplyOptions(locator);  
            
            if (!ValidateInstanceInternal(locator.InstanceId, Options.InstanceId))
            {
                return InstanceError(locator.InstanceId, Options.InstanceId);
            }

            // ReSharper disable once InconsistentlySynchronizedField
            var res1 = ProductService.Parse(locator, data);
            if (res1.IsSucceeded && res1.Result?.Products != null)
            {
                _logger.Info("Message parsed, deleting products... ");

                try
                {
                    foreach (var p in res1.Result.Products)
                    {
                        _logger.Info()
                            .Message("Deleting product {productId}...", p.Id)
                            .Property("locator", locator)
                            .Write();

                        var res2 = ProductService.DeleteProduct(locator, p.Id, data);
                        if (!res2.IsSucceeded)
                        {
                            throw new ProductException(
                                "Error while deleting product", res2.Error.Message, p.Id, locator
                            );
                        }

                        _logger.Info()
                            .Message("Product {productId} successfully deleted", p.Id)
                            .Property("locator", locator)
                            .Write();
                    }
                }
                catch (ProductException pex)
                {
                    return ProcessProductException(pex);
                }
                catch (Exception e)
                {
                    _logger.Error().Exception(e).Write();
                    return BadRequest(e.Message);
                }

                return Ok();
            }

            return ProceedParseError(data, res1);

        }

        [HttpPut("products", Name = "Put")]
        [HttpPut("products/{language}/{state}", Name = "Put-Channel")]
        [HttpPut("{customerCode}/products", Name = "Put-Consolidate")]
        [HttpPut("{customerCode}/products/{language}/{state}", Name = "Put-Channel-Consolidate")]    
        public ActionResult PutProduct([FromBody] string data, 
            [FromQuery(Name = "UserId")] int userId, [FromQuery(Name = "UserName")] string userName,
            string format = "json", string instanceId = null            
        )
        {
            var locator = new ProductLocator {QueryFormat = format, InstanceId = instanceId };         
            
            if (!ValidateInstanceInternal(locator.InstanceId, Options.InstanceId))
            {
                return InstanceError(locator.InstanceId, Options.InstanceId);
            }

            ApplyOptions(locator);
            
            // ReSharper disable once InconsistentlySynchronizedField
            var res1 = ProductService.Parse(locator, data);
            if (res1.IsSucceeded && res1.Result?.Products?.Any() == true)
            {
                _logger.Info("Message parsed, creating or updating products...  ");

                try
                {
                    foreach (var p in res1.Result.Products)
                    {
                        _logger.Info()
                            .Message("Check for creating or updating product {productId}", p.Id)
                            .Property("locator", locator)
                            .Write();
                            

                        // ReSharper disable once InconsistentlySynchronizedField
                        var res2 = ProductService.HasProductChanged(locator, p.Id, data);
                        if (!res2.IsSucceeded)
                        {
                            throw new ProductException(
                                "Error while checking product", res2.Error.Message, p.Id, locator
                            );
                        }

                        if (!res2.Result)
                        {
                            _logger.Info()
                                .Message("Product {productId} doesn't require updating", p.Id)
                                .Property("locator", locator)
                                .Write();
                        }
                        else
                        {
                            _logger.Info()
                                .Message("Creating or updating product {productId}", p.Id)
                                .Property("locator", locator)
                                .Write();

                            var res3 = ProductService.UpdateProduct(locator, p, data, userName, userId);
                            if (!res3.IsSucceeded)
                            {
                                throw new ProductException(
                                    "Error while creating/updating product", res3.Error.Message, p.Id, locator
                                );
                            }

                            _logger.Info()
                                .Message("Product {productId} successfully created/updated", p.Id)
                                .Property("locator", locator)
                                .Write();
                        }
                    }
                }
                catch (ProductException pex)
                {
                    return ProcessProductException(pex);
                }
                catch (Exception e)
                {
                    _logger.Error().Exception(e).Write();
                    return BadRequest(e.Message);
                }
                return Ok();
            }

            return ProceedParseError(data, res1);
        }

        private ActionResult ProcessProductException(ProductException pex)
        {
            _logger.Error()
                .Message(pex.Message)
                .Property("productId", pex.ProductId)
                .Property("result", pex.Result)
                .Property("locator", pex.Locator)
                .Write();
            return BadRequest($"{pex.Message}: {pex.ProductId}. {pex.Result}");
        }


        [HttpGet("products/ValidateInstance", Name = "Validate")]
        [HttpGet("products/{language}/{state}/ValidateInstance", Name = "Validate-Channel")]
        [HttpGet("products/{filterDate:datetime}/ValidateInstance", Name = "Validate-Versions")]
        [HttpGet("products/{language}/{state}/{filterDate:datetime}/ValidateInstance", Name = "Validate-Versions-Channel")]
        [HttpGet("{customerCode}/products/ValidateInstance", Name = "Validate-Consolidate")]
        [HttpGet("{customerCode}/products/{language}/{state}/ValidateInstance", Name = "Validate-Consolidate-Channel")]
        [HttpGet("{customerCode}/products/{filterDate:datetime}/ValidateInstance", Name = "Validate-Consolidate-Versions")]
        [HttpGet("{customerCode}/products/{language}/{state}/{filterDate:datetime}/ValidateInstance", Name = "Validate-Consolidate-Versions-Channel")]        
        public bool ValidateInstance(string format = "json", string instanceId = null)
        {
            var locator = new ProductLocator(){QueryFormat = format, InstanceId = instanceId };         
            ApplyOptions(locator); 
            return ValidateInstanceInternal(locator.InstanceId, Options.InstanceId);
        }

        private bool ValidateInstanceInternal(string instanceId, string actualInstanceId)
        {
            return instanceId == actualInstanceId || string.IsNullOrEmpty(instanceId) && string.IsNullOrEmpty(actualInstanceId);
        }

        private ActionResult InstanceError(string instanceId, string actualInstanceId)
        {
            _logger.Error("InstanceId {instanceId} is wrong. Must be {actualInstanceId}", instanceId, actualInstanceId);
            return new ForbiddenActionResult();
        }

        private ActionResult ProceedParseError(string data, ServiceResult<ProductInfo> res1)
        {
            string result;
            if (!res1.IsSucceeded)
            {
                result = res1.Error.Message;
                _logger.Error()
                    .Message("Could not parse message to products")
                    .Property("data", data)
                    .Property("result", result)
                    .Write();
            }
            else
            {
                result = "Could find products in parsed message";
                _logger.Error()
                    .Message(result)
                    .Property("data", data)
                    .Write();
            }
            return BadRequest(result);
        }

        private void ApplyOptions(ProductLocator locator)
        {
            locator.UseProductVersions = Options.UseProductVersions;
            
            var language = ControllerContext.RouteData.Values["language"];
            var state = ControllerContext.RouteData.Values["state"];
            string customerCode = ControllerContext.RouteData.Values.ContainsKey("customerCode") 
                ? ControllerContext.RouteData.Values["customerCode"].ToString() 
                : HttpContext.Request.Query["customerCode"].FirstOrDefault();

            locator.HeaderFormat = GetHeaderFormat(HttpContext.Request.ContentType);
            
            if (state != null)
            {
                locator.State = state.ToString();                
            }
            
            if (language != null)
            {
                locator.Language = language.ToString();
            }
            
            if (customerCode != null && customerCode != SingleCustomerCoreProvider.Key)
            {
                locator.FixedConnectionString = DBConnector.GetConnectionString(customerCode);
            }
            
            if (!String.IsNullOrEmpty(Options.FixedConnectionString))
            {
                locator.FixedConnectionString = Options.FixedConnectionString;                
            }
            
            if (String.IsNullOrEmpty(locator.FixedConnectionString))
            {
                throw new Exception("Customer code or connection string is not defined");
            }
        }
        
        private string GetHeaderFormat(string contentType)
        {
            if (contentType != null && (contentType.StartsWith("application/xml") || contentType.StartsWith("text/xml")))
            {
                return "xml";
            }
            else if (contentType != null && contentType.StartsWith("application/json"))
            {
                return "json";
            }
            else
            {
                return null;
            }          
        }
    }
}
