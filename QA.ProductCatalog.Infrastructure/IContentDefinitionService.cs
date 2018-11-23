using QA.Core.Models.Configuration;

namespace QA.ProductCatalog.Infrastructure
{
    public interface IContentDefinitionService
    {
		Content[] GetDefinitions(bool isLive = false);

        Content GetDefinitionById(int productDefinitionId, bool isLive = false);

        Content GetDefinitionForContent(int productTypeId, int contentId, bool isLive = false);
        
        /// <returns><see cref="Content"/> or null</returns>
        Content TryGetDefinitionById(int productDefinitionId, bool isLive = false);

        /// <returns><see cref="Content"/> or null</returns>
        Content TryGetDefinitionForContent(int productTypeId, int contentId, bool isLive = false);

        string GetControlDefinition(int contentId, int productTypeId);

		ServiceDefinition GetServiceDefinition(string slug, string version, bool clearExtensions = false);

        EditorDefinition GetEditorDefinition(int productTypeId, int contentId, bool isLive = false);

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

    public class EditorDefinition
    {
        public Content Content { get; set; }

        public int ProductDefinitionId { get; set; }

        public string EditorViewPath { get; set; }
    }
}
