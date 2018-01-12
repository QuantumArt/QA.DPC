
using QA.Core;
using QA.Core.DPC.Integration;
using QA.Core.Logger;
using QA.ProductCatalog.SiteSyncWebHost.ActionResults;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace QA.ProductCatalog.SiteSyncWebHost.Controllers
{
    public class ProductsController : Controller
    {
        //
        // GET: /Products/

        
        public ActionResult Send(string instanceId = null)
        {
            IDpcProductService service = ObjectFactoryBase.Resolve<IDpcProductService>();
            
            var logger = ObjectFactoryBase.Resolve<ILogger>();
            var actualInstanceId = ConfigurationManager.AppSettings["DPC.InstanceId"];

            if (instanceId != actualInstanceId)
            {
                logger.LogInfo(() => $"InstanceId {instanceId} указан неверно, должен быть {actualInstanceId}");
                return new ForbiddenActionResult();
            }

            string data = "";
            using (StreamReader inputStream = new StreamReader(Request.InputStream))
            {
                data = inputStream.ReadToEnd();
                logger.Info("Получено сообщение. Начинаем разбор... ");
            }

            //IDpcProductService service = ObjectFactoryBase.Resolve<IDpcProductService>();

            if (Request.HttpMethod == "PUT")
            {  
                var res1 = service.Parse(data);
                if (res1.IsSucceeded && res1.Result != null && res1.Result.Products != null && res1.Result.Products.Any())
                {
                    logger.Info("Сообщение разобрано, создаем/обновляем продукты... ");
                    foreach (var p in res1.Result.Products)
                    {
                        logger.Info("Проверяем, нужно ли создавать/обновлять продукт {0} ", p.Id);
                        var res2 = service.HasProductChanged(p.Id, data);
                        if (!res2.IsSucceeded)
                        {
                            logger.Info("Ошибка при проверке продукта {0}: {1}", p.Id, res2.Error.Message);
                            throw new Exception(res2.Error.Message);
                        }
                        else if (!res2.Result)
                        {
                            logger.Info("Продукт {0} не требует создания/обновления", p.Id);
                        }
                        else 
                        {
                            logger.Info("Создаем/обновляем продукт {0}", p.Id);

                            int userId = 0;
                            int.TryParse(Request.Params["UserId"], out userId);
                            var res3 = service.UpdateProduct(p, data, Request.Params["UserName"], userId);
                            if (res3.IsSucceeded)
                            {
                                logger.Info("Продукт {0} успешно создан/обновлен", p.Id);
                            }
                            else
                            {
                                logger.Info("Ошибка при создании/обновлении продукта {0}: {1}", p.Id, res3.Error.Message);
                                throw new Exception(res3.Error.Message);
                            }
                        }
                    }
                }
                else 
                {
                    logger.Info("Не удалось разобрать сообщение в продукты: {0}", data);
                    if (!res1.IsSucceeded)
                        throw new Exception(res1.Error.Message);
                    else
                        throw new Exception("Не удалось разобрать сообщение в продукты");

                }
            }
            else if (Request.HttpMethod == "DELETE")
            {
                
                var res1 = service.Parse(data);
                if (res1.IsSucceeded && res1.Result != null && res1.Result.Products != null)
                {
                    logger.Info("Сообщение разобрано, удаляем продукты ... ");
                    foreach (var p in res1.Result.Products)
                    {
                        logger.Info("Удаляем продукт {0}", p.Id);
                        var res2 = service.DeleteProduct(p.Id);
                        if (res2.IsSucceeded)
                        {
                            logger.Info("Продукт {0} успешно удален", p.Id);
                        }
                        else
                        {
                            logger.Info("Ошибка при удалении продукта {0}: {1}", p.Id, res2.Error.Message);
                            throw new Exception(res2.Error.Message);
                        }
                    }
                }
                else  
                {
                    logger.Info("Не удалось разобрать сообщение в продукты: {0}", data);
                    if (!res1.IsSucceeded)
                        throw new Exception(res1.Error.Message);
                    else
                        throw new Exception("Не удалось разобрать сообщение в продукты");
                }

            }

            return View();
        }


        public ActionResult Index()
        {
            return View();
        }
    }
}
