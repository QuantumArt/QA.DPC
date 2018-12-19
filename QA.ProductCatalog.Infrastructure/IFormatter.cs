using System.IO;
using System.Threading.Tasks;

namespace QA.ProductCatalog.Infrastructure
{
	public interface IFormatter<T>
	{
		Task<T> Read(Stream stream);
		Task Write(Stream stream, T product);
	}
}