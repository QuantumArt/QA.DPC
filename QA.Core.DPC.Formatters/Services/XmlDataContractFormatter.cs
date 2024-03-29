using QA.ProductCatalog.Infrastructure;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.DPC.Formatters.Services
{
    public class XmlDataContractFormatter<T>: IFormatter<T>
    {
        private DataContractSerializer serializer;
        
        public XmlDataContractFormatter()
        {
            serializer = new DataContractSerializer(typeof(T));
        }
        public Task<T> Read(Stream stream)
        {
            throw new System.NotImplementedException();
        }

        public Task Write(Stream stream, T product)
        {
            throw new System.NotImplementedException();
        }

        public string Serialize(T product)
        {
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, product);
                return Encoding.UTF8.GetString(ms.ToArray());
            }

        }
    }
}