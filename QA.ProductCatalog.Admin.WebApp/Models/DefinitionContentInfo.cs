using System;
using System.ComponentModel;
using QA.Core.Models.Configuration;

namespace QA.ProductCatalog.Admin.WebApp.Models
{
    public class DefinitionContentInfo : DefinitionElement
	{
        
        [DisplayName("Только для чтения ")]
        public bool IsReadOnly { get; set; }
        
        [DisplayName("Грузить все простые поля")]
        public bool LoadAllPlainFields { get; set; }

        [DisplayName("Имя контента")]
        public string ContentName { get; set; }
        
        public int ContentId { get; set; }
        
        [DisplayName("Поведение при публикации")]
        public PublishingMode PublishingMode { get; set; }
        
        public bool CacheEnabled { get; set; }
        
        public TimeSpan CachePeriod { get; set; }

		public bool IsFromDictionaries { get; set; }

		public bool AlreadyCachedAsDictionary { get; set; }
	}
}