using QA.Core.Models.Entities;

namespace QA.Core.Models
{
    /// <summary>
    /// Постобработчик модели
    /// </summary>
    /// <typeparam name="TParameter"></typeparam>
    public abstract class ModelPostProcessorBase<TParameter> : IModelPostProcessor
        where TParameter : class, new()
    {
        public ModelPostProcessorBase() : this(null) { }

        public ModelPostProcessorBase(TParameter parameter) { }

        public TParameter Parameter { get; set; }

        public bool Clone { get; set; }

        public Article ProcessModel(Article input)
        {
            return ProcessModel(input, Parameter);
        }

        protected abstract Article ProcessModel(Article input, TParameter parameter);
    }
}
