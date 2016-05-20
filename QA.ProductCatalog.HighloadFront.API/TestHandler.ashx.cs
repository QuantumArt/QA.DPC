using Mts.Sonic.Elastic;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Dependencies;

namespace Mts.Sonic.Sync.Controllers
{
    public class TestHandler : HttpTaskAsyncHandler
    {
        public static IDependencyResolver _dependencyResolver;

        public const string Json = @"
                {{
                    ""from"": {0},
                    ""size"": {1},
                    ""filter"": {{
                        ""match_all"": {{}}
                    }}
                }}";

        protected JsonSerializer Serializer { get; private set; } = new JsonSerializer();

        public override async Task ProcessRequestAsync(HttpContext context)
        {
            var page = int.Parse(context.Request["page"]);
            var perPage = int.Parse(context.Request["per_page"]);
            var lvl = int.Parse(context.Request["lvl"]);
            var client = int.Parse(context.Request["client"]);

            context.Response.ContentType = "application/json; charset=utf-8";


            if (client == 0)
            {
                var httpWebRequest = await GetWebRequestAsync(page, perPage);

                using (var httpResponse = (HttpWebResponse)await httpWebRequest.GetResponseAsync())
                using (var rs = httpResponse.GetResponseStream())
                {
                    await WriteDataAsync(rs, context.Response.OutputStream, lvl);
                }
            }
            else if (client == 1)
            {
                using (var rs = await GetClientStream(page, perPage))
                {
                    await WriteDataAsync(rs, context.Response.OutputStream, lvl);
                }
            }
            else if (client == 2)
            {
                var data = await GetClientData(page, perPage);                

                using (var writer = new StreamWriter(context.Response.OutputStream))
                {
                    Serializer.Serialize(writer, data);
                }
            }
            else
            {
                context.Response.Write("no data");
            }
        }
        
        public async Task<HttpWebRequest> GetWebRequestAsync(int page, int perPage)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://mscnosql01:9200/products/Tariff/_search");
            httpWebRequest.Proxy = null;
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(await httpWebRequest.GetRequestStreamAsync()))
            {
                await streamWriter.WriteAsync(string.Format(Json, page * perPage, perPage));
                streamWriter.Flush();
            }

            return httpWebRequest;
        }

        public async Task WriteDataAsync(Stream request, Stream response, int lvl)
        {
            if (lvl > 0)
            {
                await JsonFragmentExtractor.ExtractJsonFragment("_source", request, response, 4);
            }
            else
            {
                await request.CopyToAsync(response);
            }
        }

        public async Task<Stream> GetClientStream(int page, int perPage)
        {
            var client = (IElasticClient)_dependencyResolver.GetService(typeof(IElasticClient));

            var request = new SearchDescriptor<StreamData>()
             .Type("Tariff")
             .From(page * perPage)
             .Size(perPage);

            return await client.SearchStreamAsync(request);
        }

        public async Task<IList<JObject>> GetClientData(int page, int perPage)
        {
            var client = (IElasticClient)_dependencyResolver.GetService(typeof(IElasticClient));

            var request = new SearchDescriptor<JObject>()
             .Type("Tariff")
             .From(page * perPage)
             .Size(perPage);

            var response = await client.SearchAsync<JObject>(request);

            return response.Documents.ToList();
        }
        
    }
}