namespace QA.Core.DPC.Front;

public interface IProductSerializerFactory
{
    IProductSerializer Resolve(string format);
}
