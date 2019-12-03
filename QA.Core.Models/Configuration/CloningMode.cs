using System.ComponentModel.DataAnnotations;
using QA.Core.DPC.Resources;

namespace QA.Core.Models.Configuration
{
    public enum CloningMode 
    {
        /// <summary>
        /// при клонировании данное поле игнорируется, и устанавливается пустая ссылка.
        /// </summary>
		[Display(Name = "SetNull", ResourceType = typeof(ControlStrings))]
		Ignore = 0,
        /// <summary>
        /// При клонировании корневой сущности использовать ссылку на оригинал
        /// </summary>
        [Display(Name = "UseExisting", ResourceType = typeof(ControlStrings))]
		UseExisting = 1,
        /// <summary>
        /// При копировании корневой сущности эту тоже копировать
        /// </summary>
        [Display(Name = "Clone", ResourceType = typeof(ControlStrings))]
		Copy = 2
    }
}
