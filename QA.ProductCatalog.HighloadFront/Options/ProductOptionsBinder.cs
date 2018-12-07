using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace QA.ProductCatalog.HighloadFront.Options
{
    public class ProductOptionsBinder: IModelBinder
    {

        private SonicElasticStoreOptions _options;
        
        public ProductOptionsBinder(SonicElasticStoreOptions options)
        {
            _options = options;
        }
        
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.BinderModelName;
            if (String.IsNullOrEmpty(modelName))
            {
                return Task.CompletedTask;
            }
            
            var valueProviderResult =
                bindingContext.ValueProvider.GetValue(modelName);
            
            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(modelName,
                valueProviderResult);
            
            var value = valueProviderResult.Values;

            return Task.CompletedTask;       
        }
    }
}