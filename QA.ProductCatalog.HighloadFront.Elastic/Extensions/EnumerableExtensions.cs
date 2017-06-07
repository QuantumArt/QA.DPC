using System.Collections.Generic;

namespace QA.ProductCatalog.HighloadFront.Elastic.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Chunk<T>(
            this IEnumerable<T> source, int chunkSize)
        {
            var chunk = new List<T>(chunkSize);
            foreach (var item in source)
            {
                chunk.Add(item);
                if (chunk.Count != chunkSize) continue;
                yield return chunk;
                chunk = new List<T>(chunkSize);
            }
            if (chunk.Count > 0)
            {
                yield return chunk;
            }
        }
    }
}
