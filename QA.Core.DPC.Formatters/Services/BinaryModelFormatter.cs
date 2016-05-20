using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Formatters.Services
{
	public class BinaryModelFormatter<T> : IFormatter<T>
		where T : class
	{
		private readonly IFormatter _formatter;

		public BinaryModelFormatter()
		{
			_formatter = new BinaryFormatter();
		}

		public Task<T> Read(Stream stream)
		{
			return Task.Run<T>(() => {
				if (stream.Length == 0)
				{
					return default(T);
				}
				else
				{
					return (T)_formatter.Deserialize(stream);
				}
			});
		}

		public Task Write(Stream stream, T product)
		{
			return Task.Run(() => {
				if (product != null)
				{
					_formatter.Serialize(stream, product);
				};
			});
		}
	}
}
