﻿using NLog;
using QA.Configuration;
using QA.Core.Models.Entities;
using QA.Core.Models.UI;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.UI
{
    /// <summary>
    /// Провайдер контролов для продукта, который берет xaml-описание из контента 
    /// ожидается такой формат: CONTROL_FOR_PRODUCT_{0}, где {0} - id контента
    /// </summary>
    public class ContentBasedProductControlProvider : IProductControlProvider
    {
        private readonly IContentDefinitionService _service;
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        public ContentBasedProductControlProvider(IContentDefinitionService service)
        {
            _service = service;
        }

        public UIElement GetControlForProduct(Article product)
        {
            var text = GetString(product);

            if (!string.IsNullOrEmpty(text))
                return (UIElement)XamlConfigurationParser.CreateFrom(text);

            return null;
        }

        protected virtual string GetString(Article product)
        {
            Throws.IfArgumentNull(product, _ => product);

            int typeId = default(int);

            var typeField = product.GetField("Type");

	        var field = typeField as ExtensionArticleField;
	        
			if (field != null)
            {
                if (!int.TryParse(field.Value, out typeId))
                {
                    typeId = 0;
                }
            }

            if (typeId <= 0)
            {
                _logger.Error("Product {productId} has no type", product.Id);
            }

            return _service.GetControlDefinition(product.ContentId, typeId);
        }

    }
}
