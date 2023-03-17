using System;
using Microsoft.Extensions.DependencyInjection;

namespace QA.Core.DPC.Front;

public class ProductSerializerFactory : IProductSerializerFactory
{
    private readonly IServiceProvider _provider;
    
    public ProductSerializerFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public IProductSerializer Resolve(string format)
    {
        return format switch
        {
            "json" => _provider.GetRequiredService<JsonProductSerializer>(),
            _ => _provider.GetRequiredService<XmlProductSerializer>()
        };
    }
}
