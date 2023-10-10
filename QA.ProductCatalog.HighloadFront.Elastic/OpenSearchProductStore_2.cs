using System.Linq;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Elastic;

public class OpenSearchProductStore_2 : ElasticProductStore_8
{
    public OpenSearchProductStore_2(ElasticConfiguration config, SonicElasticStoreOptions options, ILoggerFactory loggerFactory, IProductInfoProvider productInfoProvider)
        : base(config, options, loggerFactory, productInfoProvider)
    {
    }

    protected override JsonObject GetMapping(string type, string[] fields)
    {
        JsonArray formats = new(GetDynamicDateFormatsFromConfig("Default").Select(n => JsonValue.Create(n)).ToArray());
        JsonArray templates = new(fields.Select(n => GetKeywordTemplate(type, n)).ToArray());
        templates = AddEdgeNgramTemplates(templates, type);
        templates.Add(GetTextTemplate());

        return new()
        {
            ["dynamic_date_formats"] = formats,
            ["dynamic_templates"] = templates
        };
    }
}
