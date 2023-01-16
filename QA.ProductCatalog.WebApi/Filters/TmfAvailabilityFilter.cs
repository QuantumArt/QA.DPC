using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace QA.ProductCatalog.WebApi.Filters
{
    public class TmfAvailabilityFilter : IDocumentFilter
    {
        private readonly bool _isTmfEnabled;

        public TmfAvailabilityFilter(TmfSettings tmfSettings)
        {
            _isTmfEnabled = tmfSettings?.IsEnabled ?? false;
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var api in context.ApiDescriptions)
            {
                var attribute = api.CustomAttributes().OfType<TmfProductFormatAttribute>().FirstOrDefault();

                if (attribute != null && !_isTmfEnabled)
                {
                    var route = "/" + api.RelativePath;
                    _ = swaggerDoc.Paths.Remove(route);
                }
            }
        }
    }
}
