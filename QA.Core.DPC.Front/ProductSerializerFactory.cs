namespace QA.Core.DPC.Front;

public class ProductSerializerFactory : IProductSerializerFactory
{
    private readonly IProductSerializer _jsonProductSerializer;
    private readonly IProductSerializer _xmlProductSerializer;
    
    public ProductSerializerFactory(JsonProductSerializer jsonProductSerializer, XmlProductSerializer xmlProductSerializer)
    {
        _jsonProductSerializer = jsonProductSerializer;
        _xmlProductSerializer = xmlProductSerializer;
    }

    public IProductSerializer Resolve(string format)
    {
        if (format == "json")
        {
            return _jsonProductSerializer;
        }

        return _xmlProductSerializer;
    }
}
