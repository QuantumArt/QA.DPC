using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace QA.ProductCatalog.Infrastructure
{
	public interface IFormatter<T>
	{
		Task<T> Read(Stream stream);
		Task Write(Stream stream, T product);
		string Serialize(T product);
	}
}