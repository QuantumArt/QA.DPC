﻿using QA.Core.Models.Configuration;
using System.Collections.Generic;

namespace QA.ProductCatalog.Infrastructure
{
    public interface IContentDefinitionService
    {
        Content GetDefinitionForContent(int productTypeId, int contentId, bool isLive = false);

		Content[] GetDefinitions(bool isLive = false);

        Content GetDefinitionById(int productDefinitionId, bool isLive = false);

        string GetControlDefinition(int contentId, int productTypeId);

		ServiceDefinition GetServiceDefinition(string slug, string version, bool clearExtensions = false);

        string GetDefinitionXml(int productTypeId, int contentId, bool isLive = false);

        string GetDefinitionXml(int articleId, bool isLive = false);

        /// <summary>
        /// Получить словарь со значениями полей контента ProductDefinition соответствующий
        /// контенту <paramref name="contentId"/> и типу продукта <paramref name="productTypeId"/>.
        /// </summary>
        IReadOnlyDictionary<string, string> GetDefinitionFields(int productTypeId, int contentId, bool isLive = false);

        /// <summary>
        /// Получить словарь со значениями полей контента ProductDefinition соответствующий
        /// статье <paramref name="articleId"/>.
        /// </summary>
        IReadOnlyDictionary<string, string> GetDefinitionFields(int articleId, bool isLive = false);

		void SaveDefinition(int content_item_id, string xml);
	}

	public class ServiceDefinition
	{
		public Content Content { get; set; }

		public string Filter { get; set; }

		public int[] ExstensionContentIds { get; set; }
	}
}
