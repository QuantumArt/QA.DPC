using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace QA.ProductCatalog.HighloadFront
{
    public static class JsonFragmentExtractor
    {
        public static int ReaderBufferSize { get; set; } = 4096;

        public static async Task<int> ExtractJsonFragment(string textToSearch, string text, StringBuilder responseBuilder, int? depthToSearch = null)
        {
            using (var reader = new StringReader(text))
            {
                var writer = new StringWriter(responseBuilder);
                var result = await ExtractJsonFragment(textToSearch, reader, writer, depthToSearch);
                await writer.FlushAsync();
                return result;
            }
        }

        public static async Task<int> ExtractJsonFragment(string textToSearch, TextReader reader, StringWriter writer, int? depthToSearch = null)
        {
            bool inside = false, startExport = false, exporting = false, escaped = false;
            int depth = 0, exportDepth = 0, l = 0, batchSize, count = 1, found = 0, entityNumber = 0;
            writer.Write("[");
            var findInWhole = !depthToSearch.HasValue;
            int deep = depthToSearch ?? 0;
            char[] json = new char[ReaderBufferSize];
            while ((batchSize = await reader.ReadAsync(json, 0, json.Length)) > 0)
            {
                l = l + 1;
                for (int i = 0; i < batchSize; i++)
                {
                    var c = json[i];

                    if (exporting)
                    {
                        writer.Write(c);
                        count++;
                    }

                    if (!inside && (c == '{' || c == '['))
                    {
                        if (startExport && !exporting)
                        {
                            entityNumber++;
                            exporting = true;
                            if (entityNumber > 1)
                            {
                                writer.Write(',');
                                count++;
                            }
                            writer.Write(c);
                            count++;
                        }
                        depth++;
                        continue;
                    }

                    if (!inside && (c == '}' || c == ']'))
                    {
                        depth--;
                        if (exporting && depth == exportDepth)
                        {
                            exporting = false;
                            startExport = false;
                        }
                        continue;
                    }
                    
                    if (c == '\\')
                    {
                        escaped = !escaped;
                    }
                    
                    if (c == '"' && !escaped)
                    {
                       inside = !inside;
                    }
                    
                    if (c != '\\')
                    {
                        escaped = false;
                    }

                    if (!startExport && !findInWhole && depth >= deep)
                    {
                        if (found < textToSearch.Length && c == textToSearch[found])
                        {
                            found++;
                        }
                        else
                        {
                            found = 0;
                        }

                        if (found == textToSearch.Length)
                        {
                            //нашли текст
                            // далее запись будет вестить начиная с З { или [
                            startExport = true;
                            exportDepth = depth;
                            found = 0;
                        }
                    }
                }
            }

            writer.Write("]");
            count++;

            return count;
        }      
    }    
}
