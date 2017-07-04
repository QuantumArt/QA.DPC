using System.IO;
using Nest;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class StreamResponse : SearchResponse<StreamData>
    {
        public StreamResponse(Stream stream)
        {
            Stream = stream;
        }

        public Stream Stream { get; }
    }

    public class StreamData { }
}
