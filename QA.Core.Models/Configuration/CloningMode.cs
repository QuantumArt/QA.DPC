using System.ComponentModel.DataAnnotations;

namespace QA.Core.Models.Configuration
{
    public enum CloningMode 
    {
        /// <summary>
        /// при клонировании данное поле игнорируется, и устанавливается пустая ссылка.
        /// </summary>
		[Display(Name = "устанавливать пустую ссылку")]
		Ignore = 0,
        /// <summary>
        /// При клонировании корневой сущности использовать ссылку на оригинал
        /// </summary>
		[Display(Name = "использовать ссылку на оригинал")]
		UseExisting=1,
        /// <summary>
        /// При копировании корневой сущности эту тоже копировать
        /// </summary>
		[Display(Name = "копировать сущность")]
		Copy=2
    }
}
