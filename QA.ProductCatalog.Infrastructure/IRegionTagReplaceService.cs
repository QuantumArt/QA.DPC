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
        string Replace(string text, int currentRegion, string[] exceptions = null);

        /// <summary>
        /// Получение списка тегов встречающихся в тексте
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        string[] GetTags(string text);

        /// <summary>
        /// Получение региональных тегов со значениями для данного региона
        /// </summary>
        /// <param name="currentRegion"></param>
        /// <returns></returns>
        List<RegionTag> GetRegionTags(int currentRegion);

        TagWithValues[] GetRegionTagValues(string text, int[] regionIds);
        
    }
}
