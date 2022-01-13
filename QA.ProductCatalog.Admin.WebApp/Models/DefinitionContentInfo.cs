using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.Resources;
using QA.Core.Models.Configuration;

namespace QA.ProductCatalog.Admin.WebApp.Models
{
    public class DefinitionContentInfo : DefinitionElement
    {
        public DefinitionContentInfo()
        {
        }
    
    public DefinitionContentInfo(Content content, DefinitionSearchResult searchResult, DefinitionPathInfo pathInfo)
        {
            ContentName = content.ContentName;
            IsReadOnly = content.IsReadOnly;
            PublishingMode = content.PublishingMode;
            ContentId = content.ContentId;
            LoadAllPlainFields = content.LoadAllPlainFields;
            CacheEnabled = content.CachePeriod.HasValue;
            CachePeriod = content.CachePeriod ?? new TimeSpan(1, 45, 0);
            Path = pathInfo.Path;
            Xml = pathInfo.Xml;
            InDefinition = searchResult.Found;
        }
        
        [Display(Name="IsReadOnly", ResourceType = typeof(ControlStrings))]
        public bool IsReadOnly { get; set; }
        
        [Display(Name="LoadAllPlainFields", ResourceType = typeof(ControlStrings))]
        public bool LoadAllPlainFields { get; set; }

        [Display(Name="ContentName", ResourceType = typeof(ControlStrings))]
        public string ContentName { get; set; }
        
        public int ContentId { get; set; }
        
        [Display(Name="PublishBehaviour", ResourceType = typeof(ControlStrings))]
        public PublishingMode PublishingMode { get; set; }
        
        public bool CacheEnabled { get; set; }
        
        public TimeSpan CachePeriod { get; set; }

		public bool IsFromDictionaries { get; set; }
        
        public bool IsExtensionContent { get; set; }

		public bool AlreadyCachedAsDictionary { get; set; }
	}
}