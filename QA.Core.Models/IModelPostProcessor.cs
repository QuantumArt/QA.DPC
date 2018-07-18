using QA.Core.Models.Entities;

namespace QA.Core.Models
{
    /// <summary>
    /// Постобработчик модели
    /// </summary>
    /// <typeparam name="TParameter"></typeparam>
    public interface IModelPostProcessor
    {
        Article ProcessModel(Article input);
    }
}
