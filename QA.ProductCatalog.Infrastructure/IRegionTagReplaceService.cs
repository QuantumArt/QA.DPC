using QA.Core.Models.Entities;
using System.Collections.Generic;

namespace QA.ProductCatalog.Infrastructure
{
    
    public interface IRegionTagReplaceService
    {
        /// <summary>
        /// Делает автозамену в тексте для региональных тэгов
        /// </summary>
        /// <param name="text"></param>
        /// <param name="currentRegion"></param>
        /// <param name="exceptions"></param>
        /// <returns></returns>
        string Replace(string text, int currentRegion, string[] exceptions = null, int depth = 0);

        /// <summary>
        /// Получение списка тегов встречающихся в тексте
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        string[] GetTags(string text);

        TagWithValues[] GetRegionTagValues(string text, int[] regionIds);
        
    }
}
