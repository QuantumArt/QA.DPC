using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Path;
using Microsoft.Extensions.Primitives;

namespace QA.ProductCatalog.HighloadFront.PostProcessing;

public class PostProcessHelper
{
    public static JsonNode GetRoot(JsonNode node)
    {
        var result = node;
        while (result.Parent != null)
        {
            result = result.Parent;
        }
        return result;
    }

    public static JsonNode[] Select(JsonNode node, string jsonPath)
    {
        var builder = new StringBuilder(node.GetPath());
        if (!jsonPath.StartsWith('['))
        {
            builder.Append('.');
        }
        builder.Append(jsonPath);
        var path = builder.ToString();
        var nodeList = JsonPath.Parse(path)
            .Evaluate(GetRoot(node)).Matches;
        var result = nodeList ?? NodeList.Empty;
        return result.Select(n => n.Value)
            .Where(n => n != null)
            .ToArray();
    } 
    
    
    public static JsonSerializerOptions GetSerializerOptions()
    {
        return new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    } 
    
    
    public static JsonNode CloneJsonNode(JsonNode node)
    {
        return JsonNode.Parse(node.ToJsonString(PostProcessHelper.GetSerializerOptions()));
    }
        
        
}