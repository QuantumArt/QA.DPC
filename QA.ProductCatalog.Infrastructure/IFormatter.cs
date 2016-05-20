using System.IO;
using System.Threading.Tasks;

namespace QA.ProductCatalog.Infrastructure
{
	public interface IFormatter<T>
		where T : class
	{
		Task<T> Read(Stream stream);
		Task Write(Stream stream, T product);
	}
}