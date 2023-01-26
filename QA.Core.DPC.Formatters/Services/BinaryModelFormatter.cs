using System;
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
            return Task.FromResult(
                stream.Length == 0 ? default : (T)_formatter.Deserialize(stream));
        }

        public async Task Write(Stream stream, T product)
        {
            if (product != null)
            {
                using var memoryStream = new MemoryStream();
                // TODO: Avoid using obsolete trim-incompatible serialization.
                _formatter.Serialize(memoryStream, product);
                await memoryStream.CopyToAsync(stream);
            }
        }

        public string Serialize(T product)
        {
            throw new System.NotImplementedException();
        }
    }
}
