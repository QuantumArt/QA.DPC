using QA.Core.DPC.QP.Autopublish.Models;
using QA.Core.Logger;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.QP.Autopublish.Services
{
    public class AutopublishProcessor : IAutopublishProcessor
    {
        private const int UserId = 1;
        private const string UserName = "Admin";

        private readonly IQPNotificationService _notificationService;
        private readonly IProductSimpleAPIService _productService;
        private readonly ILogger _logger;

        public AutopublishProcessor(IQPNotificationService notificationService, IProductSimpleAPIService productService, ILogger logger)
        {
            _notificationService = notificationService;
            _productService = productService;
            _logger = logger;
        }
        public int Publish(ProductItem item)
        {
            var isLive = !item.IsUnited;

            if (item.PublishAction == PublishAction.Publish)
            {
                var product = _productService.GetProduct(item.ProductId, item.DefinitionId, isLive);
                _notificationService.SendProducts(new[] { product }, isLive, UserName, UserId, true, true);
            }
            else if (item.PublishAction == PublishAction.Delete)
            {
                var product = _productService.GetAbsentProduct(item.ProductId, item.DefinitionId, isLive, item.TypeOld);
                _notificationService.DeleteProducts(new[] { product }, UserName, UserId, true);
            }

            return item.ProductId;
        }
    }
}
