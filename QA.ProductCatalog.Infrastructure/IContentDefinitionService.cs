using QA.Core.Models.Configuration;

namespace QA.ProductCatalog.Infrastructure
{
    public interface IContentDefinitionService
    {
        Content GetDefinitionForContent(int productTypeId, int contentId, bool isLive = false);

		Content[] GetDefinitions(bool isLive = false);

        string GetControlDefinition(int contentId, int productTypeId);

		ServiceDefinition GetServiceDefinition(string slug, string version, bool clearExtensions = false);

	    string GetDefinitionXml(int productTypeId, int contentId, bool isLive = false);

		string GetDefinitionXml(int articleId, bool isLive = false);

		void SaveDefinition(int content_item_id, string xml);
	}

	public class ServiceDefinition
	{
		public Content Content { get; set; }

		public string Filter { get; set; }

		public int[] ExstensionContentIds { get; set; }
	}
}
