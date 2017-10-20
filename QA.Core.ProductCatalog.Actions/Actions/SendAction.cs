using System;
using System.Threading.Tasks;
using QA.Core.Logger;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.Actions.Actions
{
	[Obsolete("Юзать вместо него SendProductAction, там есть параметры для того что бы он вел себя идентично данному")]
    public class SendAction : AsyncActionWrapper
    {
        private IProductService _productService;
        private IXmlProductService _xmlProductService;
        private IQPNotificationService _notificationService;
        #region SendAction Members

        public SendAction(ILogger logger,
            IProductService productService,
            IXmlProductService xmlProductService,
            IQPNotificationService notificationService)
            : base(logger)
        {
            _productService = productService;
            _xmlProductService = xmlProductService;
            _notificationService = notificationService;
        }

        override public async Task<string> Process(ActionContext context)
        {
            Throws.IfArgumentNull(context, _ => context);
            string id = "id" + Guid.NewGuid();

            var products = DoWithLogging("_productService.GetProductsByIds", id,
                () => _productService.GetProductsByIds(context.ContentItemIds));

            try
            {
				await _notificationService.SendProductsAsync(products, true, context.UserName, context.UserId, false, false);
            }
            catch (Exception ex)
            {
				throw new ActionException("There are some errors occured. ", new[] { ex }, context);
            }

			return null;
        }

        #endregion
    }
}
